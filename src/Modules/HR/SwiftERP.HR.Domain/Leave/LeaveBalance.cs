using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Leave;

/// <summary>One row per employee/leave-type/year. Created on first use with a default
/// entitlement — see <see cref="DefaultEntitlements"/>.</summary>
public class LeaveBalance : Entity
{
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public int Year { get; private set; }
    public int TotalDays { get; private set; }
    public int UsedDays { get; private set; }

    public int AvailableDays => TotalDays - UsedDays;

    public static readonly IReadOnlyDictionary<LeaveType, int> DefaultEntitlements = new Dictionary<LeaveType, int>
    {
        [LeaveType.Sick] = 10,
        [LeaveType.Casual] = 10,
        [LeaveType.Annual] = 20,
    };

    private LeaveBalance()
    {
    }

    public LeaveBalance(Guid employeeId, LeaveType leaveType, int year, int? totalDays = null)
    {
        EmployeeId = employeeId;
        LeaveType = leaveType;
        Year = year;
        TotalDays = totalDays ?? DefaultEntitlements[leaveType];
        UsedDays = 0;
    }

    public void UseDays(int days)
    {
        if (days <= 0)
            throw new DomainException("Days to use must be positive.");
        if (days > AvailableDays)
            throw new DomainException(
                $"Cannot use {days} day(s) of {LeaveType} leave: only {AvailableDays} available.");

        UsedDays += days;
    }

    public void RefundDays(int days)
    {
        if (days <= 0)
            throw new DomainException("Days to refund must be positive.");

        UsedDays = Math.Max(0, UsedDays - days);
    }
}
