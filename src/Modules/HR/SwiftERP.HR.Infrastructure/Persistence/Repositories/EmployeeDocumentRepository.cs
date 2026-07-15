using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class EmployeeDocumentRepository(HrDbContext dbContext) : IEmployeeDocumentRepository
{
    public void Add(EmployeeDocument document) => dbContext.EmployeeDocuments.Add(document);

    public Task<EmployeeDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.EmployeeDocuments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<List<EmployeeDocument>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken) =>
        dbContext.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync(cancellationToken);
}
