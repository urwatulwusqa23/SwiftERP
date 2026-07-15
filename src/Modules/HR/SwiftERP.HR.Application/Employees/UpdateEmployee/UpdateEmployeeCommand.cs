using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid EmployeeId,
    string? PhoneNumber,
    string? Address,
    DateOnly? DateOfBirth,
    string? JobTitle,
    string? Department,
    Guid? ManagerId) : IRequest<Result>;
