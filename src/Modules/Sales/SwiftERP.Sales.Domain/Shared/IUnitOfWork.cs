namespace SwiftERP.Sales.Domain.Shared;

/// <summary>
/// This unit of work is intentionally shared by a single SalesDbContext that also tracks
/// Inventory's Product and Finance's LedgerEntry entities for the ConfirmSaleOrder workflow.
/// One SaveChangesAsync call commits the stock decrement, ledger entry, and order confirmation
/// as a single database transaction — see SwiftERP.Sales.Infrastructure.Persistence.SalesDbContext.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
