using Microsoft.EntityFrameworkCore;
using SwiftERP.Finance.Domain.LedgerEntries;

namespace SwiftERP.Finance.Infrastructure.Persistence.Repositories;

public class LedgerRepository(FinanceDbContext dbContext) : ILedgerRepository
{
    public void Add(LedgerEntry entry) => dbContext.LedgerEntries.Add(entry);

    public Task<List<LedgerEntry>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.LedgerEntries.ToListAsync(cancellationToken);
}
