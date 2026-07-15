using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Attendance;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class AttendanceRepository(HrDbContext dbContext) : IAttendanceRepository
{
    public void Add(AttendanceRecord record) => dbContext.AttendanceRecords.Add(record);

    public Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AttendanceRecords.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AttendanceRecord?> GetOpenRecordAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId && a.ClockOutUtc == null)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<AttendanceRecord>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);
}
