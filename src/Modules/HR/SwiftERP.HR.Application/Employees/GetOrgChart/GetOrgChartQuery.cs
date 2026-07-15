using MediatR;

namespace SwiftERP.HR.Application.Employees.GetOrgChart;

/// <summary>Roots are employees with no manager (or whose manager isn't in the active set).</summary>
public record GetOrgChartQuery : IRequest<List<OrgChartNodeDto>>;
