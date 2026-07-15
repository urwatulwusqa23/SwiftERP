using MediatR;
using SwiftERP.Finance.Domain.LedgerEntries;

namespace SwiftERP.Finance.Application.LedgerEntries.GetRunningBalance;

public class GetRunningBalanceQueryHandler(ILedgerRepository ledgerRepository)
    : IRequestHandler<GetRunningBalanceQuery, decimal>
{
    public async Task<decimal> Handle(GetRunningBalanceQuery request, CancellationToken cancellationToken)
    {
        var entries = await ledgerRepository.GetAllAsync(cancellationToken);
        return entries.Sum(e => e.SignedAmount);
    }
}
