using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.Inventory.Domain.Products;
using SwiftERP.Inventory.Domain.PurchaseOrders;
using SwiftERP.Inventory.Domain.Shared;
using SwiftERP.Inventory.Infrastructure.Persistence;
using SwiftERP.Inventory.Infrastructure.Persistence.Repositories;

namespace SwiftERP.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Resolve the connection string lazily inside the options delegate (not as a variable
        // captured beforehand) so it re-reads IConfiguration at DbContext-creation time. Reading
        // it eagerly here would bake in whatever appsettings.json says before test hosts like
        // WebApplicationFactory get a chance to override ConnectionStrings:SwiftErpDb.
        services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("SwiftErpDb")
                ?? throw new InvalidOperationException("Connection string 'SwiftErpDb' is not configured.")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

        return services;
    }
}
