namespace SwiftERP.HR.Domain.Leave;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken);
    Task<List<LeaveBalance>> GetForEmployeeAsync(Guid employeeId, int year, CancellationToken cancellationToken);
    void Add(LeaveBalance balance);
}
