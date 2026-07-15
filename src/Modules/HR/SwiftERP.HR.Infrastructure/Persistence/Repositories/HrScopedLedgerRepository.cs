using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.HR.Application.Abstractions;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class HrScopedLedgerRepository(HrDbContext dbContext) : IHrLedgerPort
{
    public void AddEntry(LedgerEntry entry) => dbContext.LedgerEntries.Add(entry);
}
