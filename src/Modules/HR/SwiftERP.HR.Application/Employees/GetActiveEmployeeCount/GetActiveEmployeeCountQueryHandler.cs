using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetActiveEmployeeCount;

public class GetActiveEmployeeCountQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetActiveEmployeeCountQuery, int>
{
    public async Task<int> Handle(GetActiveEmployeeCountQuery request, CancellationToken cancellationToken)
    {
        var activeEmployees = await employeeRepository.GetActiveAsync(cancellationToken);
        return activeEmployees.Count;
    }
}
