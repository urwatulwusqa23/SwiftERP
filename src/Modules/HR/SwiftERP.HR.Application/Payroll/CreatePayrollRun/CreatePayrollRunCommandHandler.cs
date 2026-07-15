using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Payroll;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Payroll.CreatePayrollRun;

public class CreatePayrollRunCommandHandler(
    IEmployeeRepository employeeRepository,
    IPayrollRunRepository payrollRunRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePayrollRunCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var activeEmployees = await employeeRepository.GetActiveAsync(cancellationToken);
        if (activeEmployees.Count == 0)
            return Result.Failure<Guid>("Cannot create a payroll run with no active employees.");

        var lines = activeEmployees
            .Select(e => new PayrollRunLine(e.Id, e.MonthlySalary))
            .ToList();

        var payrollRun = new PayrollRun(request.Year, request.Month, lines);

        payrollRunRepository.Add(payrollRun);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(payrollRun.Id);
    }
}
