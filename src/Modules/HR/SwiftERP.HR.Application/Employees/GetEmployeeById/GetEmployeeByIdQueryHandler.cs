using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetEmployeeById;

public class GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeProfileDto?>
{
    public async Task<EmployeeProfileDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return null;

        string? managerName = null;
        if (employee.ManagerId is { } managerId)
        {
            var manager = await employeeRepository.GetByIdAsync(managerId, cancellationToken);
            managerName = manager?.FullName;
        }

        return new EmployeeProfileDto(
            employee.Id,
            employee.FullName,
            employee.Email,
            employee.MonthlySalary,
            employee.HireDate,
            employee.Status.ToString(),
            employee.PhoneNumber,
            employee.Address,
            employee.DateOfBirth,
            employee.JobTitle,
            employee.Department,
            employee.ManagerId,
            managerName);
    }
}
