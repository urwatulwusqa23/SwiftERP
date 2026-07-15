namespace SwiftERP.Sales.Domain.SaleOrders;

public interface ISaleOrderRepository
{
    Task<SaleOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> GetDraftCountAsync(CancellationToken cancellationToken);
    void Add(SaleOrder saleOrder);
}
