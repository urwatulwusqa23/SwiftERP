using MediatR;

namespace SwiftERP.Finance.Application.LedgerEntries.GetRunningBalance;

public record GetRunningBalanceQuery : IRequest<decimal>;
