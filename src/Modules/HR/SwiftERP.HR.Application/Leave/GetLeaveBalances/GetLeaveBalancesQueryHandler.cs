using MediatR;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.GetLeaveBalances;

public class GetLeaveBalancesQueryHandler(ILeaveBalanceRepository leaveBalanceRepository)
    : IRequestHandler<GetLeaveBalancesQuery, List<LeaveBalanceDto>>
{
    public async Task<List<LeaveBalanceDto>> Handle(GetLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        var existing = await leaveBalanceRepository.GetForEmployeeAsync(request.EmployeeId, request.Year, cancellationToken);
        var byType = existing.ToDictionary(b => b.LeaveType);

        // Read-only: types with no balance row yet are reported at their default entitlement
        // without persisting anything — the row is only actually created on first RequestLeave.
        return Enum.GetValues<LeaveType>()
            .Select(type => byType.TryGetValue(type, out var balance)
                ? new LeaveBalanceDto(type, request.Year, balance.TotalDays, balance.UsedDays, balance.AvailableDays)
                : new LeaveBalanceDto(type, request.Year, LeaveBalance.DefaultEntitlements[type], 0, LeaveBalance.DefaultEntitlements[type]))
            .ToList();
    }
}
