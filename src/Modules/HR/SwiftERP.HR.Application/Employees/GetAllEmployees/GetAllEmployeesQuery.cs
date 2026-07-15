using MediatR;

namespace SwiftERP.HR.Application.Employees.GetAllEmployees;

public record GetAllEmployeesQuery : IRequest<List<EmployeeSummaryDto>>;
