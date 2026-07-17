using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Inventory.Integration.Tests.Infrastructure;

public class SwiftErpApiFactory(string sqlConnectionString, string redisConnectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SwiftErpDb"] = sqlConnectionString,
                ["ConnectionStrings:Redis"] = redisConnectionString
            });
        });
    }

    // Every endpoint now requires a bearer token, so these end-to-end tests need one too. Rather
    // than hardcode a JWT signing key here (which would drift from appsettings.json), mint the
    // token through the app's own DI-registered ITokenIssuer — same code path a real login uses,
    // just skipping the password check since these tests aren't exercising auth itself.
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var tokenIssuer = scope.ServiceProvider.GetRequiredService<ITokenIssuer>();
        var permissions = Enum.GetValues<Module>().ToDictionary(m => m, _ => AccessLevel.Full);
        var issued = tokenIssuer.Issue(Guid.NewGuid(), Guid.NewGuid(), "test-runner@swifterp.local", permissions, isSystemAdmin: true);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", issued.Token);
        return client;
    }
}
