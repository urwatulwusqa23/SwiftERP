namespace SwiftERP.Finance.Domain.LedgerEntries;

public interface ILedgerRepository
{
    void Add(LedgerEntry entry);
    Task<List<LedgerEntry>> GetAllAsync(CancellationToken cancellationToken);
}
