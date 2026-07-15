using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SwiftERP.Api.Endpoints;
using SwiftERP.HR.Domain.Payroll;
using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Api.Notifications;

/// <summary>
/// Explicit cache-aside invalidation: a low-stock crossing or a payroll posting both change a
/// number shown on the dashboard, so drop the cached summary immediately rather than waiting out
/// the 30s TTL in DashboardEndpoints.
/// </summary>
public class DashboardCacheInvalidationHandler(IDistributedCache cache) :
    INotificationHandler<ProductLowStockEvent>,
    INotificationHandler<PayrollRunPostedEvent>
{
    public Task Handle(ProductLowStockEvent notification, CancellationToken cancellationToken) =>
        cache.RemoveAsync(DashboardEndpoints.CacheKey, cancellationToken);

    public Task Handle(PayrollRunPostedEvent notification, CancellationToken cancellationToken) =>
        cache.RemoveAsync(DashboardEndpoints.CacheKey, cancellationToken);
}
