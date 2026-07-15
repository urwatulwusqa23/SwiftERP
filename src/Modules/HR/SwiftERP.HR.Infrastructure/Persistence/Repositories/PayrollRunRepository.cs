using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Payroll;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class PayrollRunRepository(HrDbContext dbContext) : IPayrollRunRepository
{
    public Task<PayrollRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PayrollRuns
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(PayrollRun payrollRun) => dbContext.PayrollRuns.Add(payrollRun);
}
