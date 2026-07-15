using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class LeaveRequestRepository(HrDbContext dbContext) : ILeaveRequestRepository
{
    public Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<LeaveRequest>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.LeaveRequests.Where(r => r.EmployeeId == employeeId).ToListAsync(cancellationToken);

    public void Add(LeaveRequest request) => dbContext.LeaveRequests.Add(request);
}
