namespace SwiftERP.HR.Application.Attendance.GetAttendance;

public record AttendanceRecordDto(
    Guid Id,
    DateOnly Date,
    DateTimeOffset ClockInUtc,
    DateTimeOffset? ClockOutUtc,
    double? WorkedHours,
    double? OvertimeHours);
