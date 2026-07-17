using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Finance.Domain.Shared;
using SwiftERP.Finance.Infrastructure.Persistence;
using SwiftERP.Finance.Infrastructure.Persistence.Repositories;

namespace SwiftERP.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // See SwiftERP.Inventory.Infrastructure.DependencyInjection for why this reads the
        // connection string lazily inside the options delegate rather than as a captured variable.
        services.AddDbContext<FinanceDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("SwiftErpDb")
                ?? throw new InvalidOperationException("Connection string 'SwiftErpDb' is not configured.")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FinanceDbContext>());
        services.AddScoped<ILedgerRepository, LedgerRepository>();

        return services;
    }
}
