using SwiftERP.SharedKernel;

namespace SwiftERP.Finance.Domain.LedgerEntries;

public class LedgerEntry : Entity
{
    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = default!;
    public Guid ReferenceId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    private LedgerEntry()
    {
    }

    public LedgerEntry(LedgerEntryType type, decimal amount, string description, Guid referenceId)
    {
        if (amount <= 0)
            throw new DomainException("Ledger entry amount must be positive.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Ledger entry description cannot be empty.");

        Type = type;
        Amount = amount;
        Description = description;
        ReferenceId = referenceId;
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }

    /// Revenue entries increase the balance; expense entries decrease it.
    public decimal SignedAmount => Type == LedgerEntryType.SaleRevenue ? Amount : -Amount;
}
