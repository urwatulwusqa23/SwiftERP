namespace SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;

public record SaleOrderLineDto(Guid ProductId, int Quantity, decimal UnitPrice);

public record SaleOrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string PaymentStatus,
    decimal Total,
    List<SaleOrderLineDto> Lines);
