using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.GetLeaveRequests;

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    string? Reason,
    LeaveRequestStatus Status,
    DateTimeOffset RequestedAtUtc);
