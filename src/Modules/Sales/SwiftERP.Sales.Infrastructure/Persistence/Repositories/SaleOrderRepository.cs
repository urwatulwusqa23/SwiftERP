using Microsoft.EntityFrameworkCore;
using SwiftERP.Sales.Domain.SaleOrders;

namespace SwiftERP.Sales.Infrastructure.Persistence.Repositories;

public class SaleOrderRepository(SalesDbContext dbContext) : ISaleOrderRepository
{
    public Task<SaleOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SaleOrders
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, cancellationToken);

    public Task<List<SaleOrder>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.SaleOrders
            .Include(so => so.Lines)
            .OrderByDescending(so => so.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<int> GetDraftCountAsync(CancellationToken cancellationToken) =>
        dbContext.SaleOrders.CountAsync(so => so.Status == SaleOrderStatus.Draft, cancellationToken);

    public void Add(SaleOrder saleOrder) => dbContext.SaleOrders.Add(saleOrder);
}
