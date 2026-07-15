using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class EmployeeRepository(HrDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<Employee>> GetActiveAsync(CancellationToken cancellationToken) =>
        dbContext.Employees.Where(e => e.Status == EmploymentStatus.Active).ToListAsync(cancellationToken);

    public Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.Employees.ToListAsync(cancellationToken);

    public void Add(Employee employee) => dbContext.Employees.Add(employee);
}
