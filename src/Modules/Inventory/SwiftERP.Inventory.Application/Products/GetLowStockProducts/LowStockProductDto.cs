namespace SwiftERP.Inventory.Application.Products.GetLowStockProducts;

public record LowStockProductDto(Guid Id, string Sku, string Name, int QuantityOnHand, int ReorderThreshold);
