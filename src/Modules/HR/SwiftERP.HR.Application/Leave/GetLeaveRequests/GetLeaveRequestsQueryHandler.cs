using MediatR;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.GetLeaveRequests;

public class GetLeaveRequestsQueryHandler(ILeaveRequestRepository leaveRequestRepository)
    : IRequestHandler<GetLeaveRequestsQuery, List<LeaveRequestDto>>
{
    public async Task<List<LeaveRequestDto>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await leaveRequestRepository.GetForEmployeeAsync(request.EmployeeId, cancellationToken);

        return requests
            .Select(r => new LeaveRequestDto(
                r.Id, r.EmployeeId, r.LeaveType, r.StartDate, r.EndDate, r.TotalDays, r.Reason, r.Status, r.RequestedAtUtc))
            .OrderByDescending(r => r.RequestedAtUtc)
            .ToList();
    }
}
