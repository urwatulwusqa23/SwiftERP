using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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
}
