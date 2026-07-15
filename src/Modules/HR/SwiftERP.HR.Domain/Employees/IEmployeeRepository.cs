namespace SwiftERP.HR.Domain.Employees;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Employee>> GetActiveAsync(CancellationToken cancellationToken);
    Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken);
    void Add(Employee employee);
}
