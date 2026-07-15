namespace SwiftERP.HR.Application.Employees.GetEmployeeById;

public record EmployeeProfileDto(
    Guid Id,
    string FullName,
    string Email,
    decimal MonthlySalary,
    DateOnly HireDate,
    string Status,
    string? PhoneNumber,
    string? Address,
    DateOnly? DateOfBirth,
    string? JobTitle,
    string? Department,
    Guid? ManagerId,
    string? ManagerName);
