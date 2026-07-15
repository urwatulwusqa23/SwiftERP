namespace SwiftERP.HR.Domain.Employees;

public interface IEmployeeDocumentRepository
{
    void Add(EmployeeDocument document);
    Task<EmployeeDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<EmployeeDocument>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
}
