using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.SharedKernel;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Identity.Domain.Shared;
using SwiftERP.Identity.Domain.Users;
using SwiftERP.Identity.Infrastructure.Persistence;
using SwiftERP.Identity.Infrastructure.Persistence.Repositories;
using SwiftERP.Identity.Infrastructure.Security;
using SwiftERP.Identity.Infrastructure.Seeding;

namespace SwiftERP.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // See SwiftERP.Inventory.Infrastructure.DependencyInjection for why the connection string
        // is read lazily inside the options delegate rather than as a captured variable.
        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(
            PostgresConnectionString.Normalize(
                configuration.GetConnectionString("SwiftErpDb")
                    ?? throw new InvalidOperationException("Connection string 'SwiftErpDb' is not configured."))));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();

        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
