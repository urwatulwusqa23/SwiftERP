using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.UpdateEmployee;

public class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure($"Employee '{request.EmployeeId}' was not found.");

        if (request.ManagerId is { } managerId)
        {
            var manager = await employeeRepository.GetByIdAsync(managerId, cancellationToken);
            if (manager is null)
                return Result.Failure($"Manager '{managerId}' was not found.");
        }

        employee.UpdatePersonalInfo(request.PhoneNumber, request.Address, request.DateOfBirth);
        employee.UpdateJobInfo(request.JobTitle, request.Department, request.ManagerId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
