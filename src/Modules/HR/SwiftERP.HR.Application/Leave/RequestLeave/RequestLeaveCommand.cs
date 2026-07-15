using MediatR;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.RequestLeave;

public record RequestLeaveCommand(
    Guid EmployeeId,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Reason) : IRequest<Result<Guid>>;
