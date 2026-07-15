namespace SwiftERP.HR.Domain.Payroll;

public interface IPayrollRunRepository
{
    Task<PayrollRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(PayrollRun payrollRun);
}
