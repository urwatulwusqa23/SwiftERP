using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetOrgChart;

public class GetOrgChartQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetOrgChartQuery, List<OrgChartNodeDto>>
{
    public async Task<List<OrgChartNodeDto>> Handle(GetOrgChartQuery request, CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetActiveAsync(cancellationToken);
        var byId = employees.ToDictionary(e => e.Id);
        var byManager = employees
            .Where(e => e.ManagerId.HasValue)
            .GroupBy(e => e.ManagerId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var roots = employees.Where(e => !e.ManagerId.HasValue || !byId.ContainsKey(e.ManagerId.Value));

        return roots.Select(e => BuildNode(e, byManager, [])).ToList();
    }

    private static OrgChartNodeDto BuildNode(
        Employee employee,
        Dictionary<Guid, List<Employee>> byManager,
        HashSet<Guid> visited)
    {
        // Guards against a manager cycle (A reports to B, B reports to A) turning this into
        // infinite recursion — the domain only rejects direct self-management, not longer cycles.
        if (!visited.Add(employee.Id))
            return new OrgChartNodeDto(employee.Id, employee.FullName, employee.JobTitle, employee.Department, []);

        var reports = byManager.TryGetValue(employee.Id, out var direct)
            ? direct.Select(r => BuildNode(r, byManager, visited)).ToList()
            : [];

        return new OrgChartNodeDto(employee.Id, employee.FullName, employee.JobTitle, employee.Department, reports);
    }
}
