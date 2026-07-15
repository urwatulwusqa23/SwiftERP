namespace SwiftERP.HR.Domain.Leave;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<LeaveRequest>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
    void Add(LeaveRequest request);
}
