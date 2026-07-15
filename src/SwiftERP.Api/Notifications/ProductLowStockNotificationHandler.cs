using MediatR;
using SwiftERP.Inventory.Domain.Products;

namespace SwiftERP.Api.Notifications;

public class ProductLowStockNotificationHandler(INotificationPublisher publisher)
    : INotificationHandler<ProductLowStockEvent>
{
    public Task Handle(ProductLowStockEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(
            "stock.low",
            new
            {
                productId = notification.ProductId,
                sku = notification.Sku,
                quantityOnHand = notification.QuantityOnHand,
                reorderThreshold = notification.ReorderThreshold
            },
            cancellationToken);
}
