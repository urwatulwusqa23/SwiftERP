using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Leave;

public enum LeaveRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class LeaveRequest : Entity
{
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? Reason { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public string? DecisionNote { get; private set; }

    public int TotalDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    private LeaveRequest()
    {
    }

    public LeaveRequest(Guid employeeId, LeaveType leaveType, DateOnly startDate, DateOnly endDate, string? reason)
    {
        if (endDate < startDate)
            throw new DomainException("Leave end date cannot be before the start date.");

        EmployeeId = employeeId;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason;
        Status = LeaveRequestStatus.Pending;
        RequestedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Approve()
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new DomainException($"Cannot approve a leave request in status '{Status}'.");

        Status = LeaveRequestStatus.Approved;
        DecidedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reject(string? note)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new DomainException($"Cannot reject a leave request in status '{Status}'.");

        Status = LeaveRequestStatus.Rejected;
        DecisionNote = note;
        DecidedAtUtc = DateTimeOffset.UtcNow;
    }
}
