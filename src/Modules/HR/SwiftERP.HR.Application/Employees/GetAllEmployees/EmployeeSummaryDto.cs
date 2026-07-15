namespace SwiftERP.HR.Application.Employees.GetAllEmployees;

public record EmployeeSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string Status,
    string? JobTitle,
    string? Department,
    Guid? ManagerId);
