using SwiftERP.Finance.Domain.LedgerEntries;

namespace SwiftERP.HR.Application.Abstractions;

/// <summary>
/// HR-owned port onto the LedgerEntry aggregate for the PostPayrollRun workflow — distinct from
/// Finance's own <see cref="ILedgerRepository"/> so the two modules' Infrastructure registrations
/// don't collide in the shared DI container (same pattern as Sales' ISalesLedgerPort).
/// </summary>
public interface IHrLedgerPort
{
    void AddEntry(LedgerEntry entry);
}
