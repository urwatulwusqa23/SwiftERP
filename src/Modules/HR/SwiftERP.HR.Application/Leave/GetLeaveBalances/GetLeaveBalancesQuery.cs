using MediatR;

namespace SwiftERP.HR.Application.Leave.GetLeaveBalances;

public record GetLeaveBalancesQuery(Guid EmployeeId, int Year) : IRequest<List<LeaveBalanceDto>>;
