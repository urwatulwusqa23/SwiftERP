using MediatR;

namespace SwiftERP.HR.Application.Attendance.GetAttendance;

public record GetAttendanceForEmployeeQuery(Guid EmployeeId) : IRequest<List<AttendanceRecordDto>>;
