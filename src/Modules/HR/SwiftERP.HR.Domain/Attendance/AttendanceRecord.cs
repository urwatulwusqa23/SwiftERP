using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Attendance;

public class AttendanceRecord : Entity
{
    public const double StandardShiftHours = 8.0;

    public Guid EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTimeOffset ClockInUtc { get; private set; }
    public DateTimeOffset? ClockOutUtc { get; private set; }

    public double? WorkedHours => ClockOutUtc.HasValue
        ? Math.Round((ClockOutUtc.Value - ClockInUtc).TotalHours, 2)
        : null;

    public double? OvertimeHours => WorkedHours.HasValue
        ? Math.Round(Math.Max(0, WorkedHours.Value - StandardShiftHours), 2)
        : null;

    private AttendanceRecord()
    {
    }

    public AttendanceRecord(Guid employeeId, DateOnly date, DateTimeOffset clockInUtc)
    {
        EmployeeId = employeeId;
        Date = date;
        ClockInUtc = clockInUtc;
    }

    public void ClockOut(DateTimeOffset clockOutUtc)
    {
        if (ClockOutUtc.HasValue)
            throw new DomainException("This attendance record has already been clocked out.");
        if (clockOutUtc <= ClockInUtc)
            throw new DomainException("Clock-out time must be after clock-in time.");

        ClockOutUtc = clockOutUtc;
    }
}
