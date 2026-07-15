namespace SwiftERP.HR.Domain.Attendance;

public interface IAttendanceRepository
{
    void Add(AttendanceRecord record);
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AttendanceRecord?> GetOpenRecordAsync(Guid employeeId, CancellationToken cancellationToken);
    Task<List<AttendanceRecord>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
}
