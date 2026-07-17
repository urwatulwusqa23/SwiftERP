using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.Sales.Application.Abstractions;
using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.Sales.Domain.Shared;
using SwiftERP.Sales.Infrastructure.Persistence;
using SwiftERP.Sales.Infrastructure.Persistence.Repositories;

namespace SwiftERP.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // See SwiftERP.Inventory.Infrastructure.DependencyInjection for why this reads the
        // connection string lazily inside the options delegate rather than as a captured variable.
        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("SwiftErpDb")
                ?? throw new InvalidOperationException("Connection string 'SwiftErpDb' is not configured.")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SalesDbContext>());
        services.AddScoped<ISaleOrderRepository, SaleOrderRepository>();
        services.AddScoped<ISalesInventoryPort, SalesScopedProductRepository>();
        services.AddScoped<ISalesLedgerPort, SalesScopedLedgerRepository>();

        return services;
    }
}
