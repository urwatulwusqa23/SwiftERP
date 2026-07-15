using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetAllEmployees;

public class GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetAllEmployeesQuery, List<EmployeeSummaryDto>>
{
    public async Task<List<EmployeeSummaryDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetAllAsync(cancellationToken);

        return employees
            .Select(e => new EmployeeSummaryDto(e.Id, e.FullName, e.Email, e.Status.ToString(), e.JobTitle, e.Department, e.ManagerId))
            .ToList();
    }
}
