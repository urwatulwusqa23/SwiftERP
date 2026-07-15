using MediatR;

namespace SwiftERP.HR.Application.Employees.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid EmployeeId) : IRequest<EmployeeProfileDto?>;
