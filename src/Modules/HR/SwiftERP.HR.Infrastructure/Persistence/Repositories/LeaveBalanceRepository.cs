using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class LeaveBalanceRepository(HrDbContext dbContext) : ILeaveBalanceRepository
{
    public Task<LeaveBalance?> GetAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken) =>
        dbContext.LeaveBalances.FirstOrDefaultAsync(
            b => b.EmployeeId == employeeId && b.LeaveType == leaveType && b.Year == year, cancellationToken);

    public Task<List<LeaveBalance>> GetForEmployeeAsync(Guid employeeId, int year, CancellationToken cancellationToken) =>
        dbContext.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .ToListAsync(cancellationToken);

    public void Add(LeaveBalance balance) => dbContext.LeaveBalances.Add(balance);
}
