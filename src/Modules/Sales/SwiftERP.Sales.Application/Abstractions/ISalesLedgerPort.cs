using SwiftERP.Finance.Domain.LedgerEntries;

namespace SwiftERP.Sales.Application.Abstractions;

/// <summary>
/// Sales-owned port onto the LedgerEntry aggregate — see <see cref="ISalesInventoryPort"/> for why
/// this is a distinct interface from Finance's own <see cref="ILedgerRepository"/>.
/// </summary>
public interface ISalesLedgerPort
{
    void AddEntry(LedgerEntry entry);
}
