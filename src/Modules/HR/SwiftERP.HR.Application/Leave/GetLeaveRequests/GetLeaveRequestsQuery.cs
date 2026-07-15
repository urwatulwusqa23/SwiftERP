using MediatR;

namespace SwiftERP.HR.Application.Leave.GetLeaveRequests;

public record GetLeaveRequestsQuery(Guid EmployeeId) : IRequest<List<LeaveRequestDto>>;
