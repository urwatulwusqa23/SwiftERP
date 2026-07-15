using MediatR;
using SwiftERP.HR.Domain.Attendance;

namespace SwiftERP.HR.Application.Attendance.GetAttendance;

public class GetAttendanceForEmployeeQueryHandler(IAttendanceRepository attendanceRepository)
    : IRequestHandler<GetAttendanceForEmployeeQuery, List<AttendanceRecordDto>>
{
    public async Task<List<AttendanceRecordDto>> Handle(GetAttendanceForEmployeeQuery request, CancellationToken cancellationToken)
    {
        var records = await attendanceRepository.GetForEmployeeAsync(request.EmployeeId, cancellationToken);

        return records
            .Select(r => new AttendanceRecordDto(r.Id, r.Date, r.ClockInUtc, r.ClockOutUtc, r.WorkedHours, r.OvertimeHours))
            .OrderByDescending(r => r.ClockInUtc)
            .ToList();
    }
}
