using SwiftERP.HR.Domain.Attendance;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Attendance;

public class AttendanceRecordTests
{
    [Fact]
    public void Constructor_SetsClockInAndLeavesClockOutNull()
    {
        var clockIn = new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);
        var record = new AttendanceRecord(Guid.NewGuid(), DateOnly.FromDateTime(clockIn.UtcDateTime), clockIn);

        Assert.Null(record.ClockOutUtc);
        Assert.Null(record.WorkedHours);
        Assert.Null(record.OvertimeHours);
    }

    [Fact]
    public void ClockOut_AfterClockIn_ComputesWorkedHours()
    {
        var clockIn = new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);
        var clockOut = clockIn.AddHours(6);
        var record = new AttendanceRecord(Guid.NewGuid(), DateOnly.FromDateTime(clockIn.UtcDateTime), clockIn);

        record.ClockOut(clockOut);

        Assert.Equal(6.0, record.WorkedHours);
        Assert.Equal(0.0, record.OvertimeHours);
    }

    [Fact]
    public void ClockOut_BeyondStandardShift_ComputesOvertimeHours()
    {
        var clockIn = new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);
        var clockOut = clockIn.AddHours(10);
        var record = new AttendanceRecord(Guid.NewGuid(), DateOnly.FromDateTime(clockIn.UtcDateTime), clockIn);

        record.ClockOut(clockOut);

        Assert.Equal(10.0, record.WorkedHours);
        Assert.Equal(2.0, record.OvertimeHours);
    }

    [Fact]
    public void ClockOut_BeforeOrAtClockIn_Throws()
    {
        var clockIn = new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);
        var record = new AttendanceRecord(Guid.NewGuid(), DateOnly.FromDateTime(clockIn.UtcDateTime), clockIn);

        Assert.Throws<DomainException>(() => record.ClockOut(clockIn));
        Assert.Throws<DomainException>(() => record.ClockOut(clockIn.AddMinutes(-5)));
    }

    [Fact]
    public void ClockOut_WhenAlreadyClockedOut_Throws()
    {
        var clockIn = new DateTimeOffset(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);
        var record = new AttendanceRecord(Guid.NewGuid(), DateOnly.FromDateTime(clockIn.UtcDateTime), clockIn);
        record.ClockOut(clockIn.AddHours(4));

        Assert.Throws<DomainException>(() => record.ClockOut(clockIn.AddHours(5)));
    }
}
