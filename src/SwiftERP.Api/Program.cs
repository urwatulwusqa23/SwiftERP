using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using SwiftERP.Api.Endpoints;
using SwiftERP.Api.Middleware;
using SwiftERP.Api.Notifications;
using SwiftERP.Finance.Application;
using SwiftERP.Finance.Infrastructure;
using SwiftERP.HR.Application;
using SwiftERP.HR.Infrastructure;
using SwiftERP.Identity.Application;
using SwiftERP.Identity.Infrastructure;
using SwiftERP.Identity.Infrastructure.Security;
using SwiftERP.Inventory.Application;
using SwiftERP.Inventory.Infrastructure;
using SwiftERP.Sales.Application;
using SwiftERP.Sales.Infrastructure;
using SwiftERP.SharedKernel;

// Bootstrap logger only for startup diagnostics before DI is available. Registered via
// AddSerilog (DI) rather than Host.UseSerilog so it never touches the shared static
// Log.Logger — WebApplicationFactory/dotnet-ef intentionally abort host builds with a
// HostAbortedException, and Host.UseSerilog + Log.CloseAndFlush() in that path would
// otherwise close the *shared* logger out from under a still-running test host.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    const string DevFrontendCorsPolicy = "DevFrontend";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(DevFrontendCorsPolicy, policy => policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
    });

    // All enums serialize as their string names (e.g. "Contract" not 0) — consistent, readable
    // JSON for every module rather than each DTO manually calling .ToString() on its enums.
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddHealthChecks();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "SwiftERP API", Version = "v1" });
    });

    builder.Services.AddInventoryApplication();
    builder.Services.AddInventoryInfrastructure(builder.Configuration);

    builder.Services.AddFinanceApplication();
    builder.Services.AddFinanceInfrastructure(builder.Configuration);

    builder.Services.AddSalesApplication();
    builder.Services.AddSalesInfrastructure(builder.Configuration);

    builder.Services.AddHrApplication();
    builder.Services.AddHrInfrastructure(builder.Configuration);

    builder.Services.AddIdentityApplication();
    builder.Services.AddIdentityInfrastructure(builder.Configuration);

    // Signing key is read here (not lazily) only to configure token *validation* parameters,
    // which is a one-time startup concern distinct from the lazy-connection-string pattern used
    // for DbContexts — there's no WebApplicationFactory override scenario for this value.
    var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
        ?? throw new InvalidOperationException("Configuration 'Jwt:SigningKey' is not set.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SwiftERP";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SwiftERP";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };

            // EventSource (used for the SSE notification stream) can't set an Authorization
            // header, so that one path accepts the token as a query param instead — same
            // validation, just a different place to read it from.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Path == "/api/v1/notifications/stream"
                        && context.Request.Query.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
            };
        });
    builder.Services.AddAuthorization();

    // Login is the one endpoint an attacker can hammer without a token, so it gets its own
    // per-IP fixed window rather than relying on the (currently nonexistent) account lockout —
    // 10 attempts/minute is generous for a real user mistyping a password, punishing for a
    // credential-stuffing script.
    const string LoginRateLimitPolicy = "LoginRateLimit";
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(LoginRateLimitPolicy, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
    });

    // Options delegates (AddStackExchangeRedisCache) and DI factory delegates (AddSingleton) both
    // resolve lazily on first use, not at registration time, so — unlike the DbContext connection
    // string bug fixed in Phase 2 — reading configuration here is safe for test-time overrides.
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = builder.Configuration.GetConnectionString("Redis"));

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(sp.GetRequiredService<IConfiguration>().GetConnectionString("Redis")!));

    builder.Services.AddSingleton<INotificationPublisher, RedisNotificationPublisher>();

    // Discovers the Api-layer INotificationHandler<T> classes in SwiftERP.Api.Notifications.
    // MediatR supports multiple handlers per notification type, so this coexists cleanly with
    // each module's own AddMediatR call registered above.
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors(DevFrontendCorsPolicy);

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    // Best-effort: a fresh dev DB with no roles/admin yet shouldn't block the API from starting,
    // and a WebApplicationFactory-based test host has no real SQL Server behind it at all.
    try
    {
        using var seedScope = app.Services.CreateScope();
        await seedScope.ServiceProvider.GetRequiredService<SwiftERP.Identity.Infrastructure.Seeding.IdentitySeeder>()
            .SeedAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Identity seeding skipped — database may not be reachable yet.");
    }

    app.MapHealthChecks("/health").AllowAnonymous();

    app.MapIdentityEndpoints();
    app.MapInventoryEndpoints();
    app.MapSalesEndpoints();
    app.MapFinanceEndpoints();
    app.MapHrEndpoints();
    app.MapDashboardEndpoints();
    app.MapNotificationStreamEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "SwiftERP.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
