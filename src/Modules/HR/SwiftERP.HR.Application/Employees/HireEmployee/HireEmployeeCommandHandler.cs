using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.HireEmployee;

public class HireEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<HireEmployeeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(HireEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (request.ManagerId is { } managerId)
        {
            var manager = await employeeRepository.GetByIdAsync(managerId, cancellationToken);
            if (manager is null)
                return Result.Failure<Guid>($"Manager '{managerId}' was not found.");
        }

        var employee = new Employee(
            request.FullName,
            request.Email,
            request.MonthlySalary,
            request.HireDate,
            request.JobTitle,
            request.Department,
            request.ManagerId);

        employeeRepository.Add(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(employee.Id);
    }
}
