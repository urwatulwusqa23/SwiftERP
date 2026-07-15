using MediatR;

namespace SwiftERP.HR.Application.Employees.GetActiveEmployeeCount;

public record GetActiveEmployeeCountQuery : IRequest<int>;
