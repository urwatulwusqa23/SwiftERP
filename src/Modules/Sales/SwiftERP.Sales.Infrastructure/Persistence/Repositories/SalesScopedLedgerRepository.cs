using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Sales.Application.Abstractions;

namespace SwiftERP.Sales.Infrastructure.Persistence.Repositories;

/// <summary>
/// Writes to the same LedgerEntries table as Finance's own repository, but through
/// SalesDbContext — see <see cref="SalesScopedProductRepository"/> for why.
/// </summary>
public class SalesScopedLedgerRepository(SalesDbContext dbContext) : ISalesLedgerPort
{
    public void AddEntry(LedgerEntry entry) => dbContext.LedgerEntries.Add(entry);
}
