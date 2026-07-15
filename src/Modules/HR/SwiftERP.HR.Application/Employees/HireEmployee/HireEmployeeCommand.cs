using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.HireEmployee;

public record HireEmployeeCommand(
    string FullName,
    string Email,
    decimal MonthlySalary,
    DateOnly HireDate,
    string? JobTitle = null,
    string? Department = null,
    Guid? ManagerId = null) : IRequest<Result<Guid>>;
