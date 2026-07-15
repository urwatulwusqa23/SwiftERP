namespace SwiftERP.HR.Application.Employees.GetOrgChart;

public record OrgChartNodeDto(
    Guid Id,
    string FullName,
    string? JobTitle,
    string? Department,
    List<OrgChartNodeDto> Reports);
