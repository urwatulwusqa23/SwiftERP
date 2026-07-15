using System.Net;
using System.Net.Http.Json;
using SwiftERP.HR.Application.Employees.HireEmployee;
using SwiftERP.HR.Application.Leave.RequestLeave;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.HR;

/// <summary>
/// Proves the leave approval workflow actually deducts the balance (not just flips a status),
/// and that rejecting a request never touches the balance at all.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class LeaveManagementTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private SwiftErpApiFactory _factory = default!;
    private HttpClient _client = default!;

    public Task InitializeAsync()
    {
        _factory = new SwiftErpApiFactory(fixture.ConnectionString, fixture.RedisConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<Guid> HireEmployeeAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/hr/employees",
            new HireEmployeeCommand($"Leave Test {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", 3500m, new DateOnly(2026, 1, 1)));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task ApproveLeaveRequest_DeductsBalance()
    {
        var employeeId = await HireEmployeeAsync();

        var requestResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/leave-requests",
            new RequestLeaveCommand(employeeId, LeaveType.Annual, new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 5), "Trip"));
        Assert.Equal(HttpStatusCode.Created, requestResponse.StatusCode);
        var leaveRequest = await requestResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var approveResponse = await _client.PostAsync($"/api/v1/hr/leave-requests/{leaveRequest!.Id}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResponse.StatusCode);

        var balances = await _client.GetFromJsonAsync<List<LeaveBalanceView>>(
            $"/api/v1/hr/employees/{employeeId}/leave-balances?year=2026");
        var annual = balances!.Single(b => b.LeaveType == "Annual");

        Assert.Equal(3, annual.UsedDays); // Aug 3-5 inclusive
        Assert.Equal(LeaveBalance.DefaultEntitlements[LeaveType.Annual] - 3, annual.AvailableDays);
    }

    [Fact]
    public async Task RejectLeaveRequest_DoesNotAffectBalance()
    {
        var employeeId = await HireEmployeeAsync();

        var requestResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/leave-requests",
            new RequestLeaveCommand(employeeId, LeaveType.Sick, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2), "Flu"));
        var leaveRequest = await requestResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/v1/hr/leave-requests/{leaveRequest!.Id}/reject", new { note = "Peak season" });
        Assert.Equal(HttpStatusCode.NoContent, rejectResponse.StatusCode);

        var balances = await _client.GetFromJsonAsync<List<LeaveBalanceView>>(
            $"/api/v1/hr/employees/{employeeId}/leave-balances?year=2026");
        var sick = balances!.Single(b => b.LeaveType == "Sick");

        Assert.Equal(0, sick.UsedDays);
        Assert.Equal(LeaveBalance.DefaultEntitlements[LeaveType.Sick], sick.AvailableDays);
    }

    [Fact]
    public async Task RequestLeave_ExceedingAvailableBalance_ReturnsBadRequest()
    {
        var employeeId = await HireEmployeeAsync();

        // Casual default entitlement is 10 days; request 11.
        var response = await _client.PostAsJsonAsync(
            "/api/v1/hr/leave-requests",
            new RequestLeaveCommand(employeeId, LeaveType.Casual, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 11), null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record CreatedResponse(Guid Id);
    private record LeaveBalanceView(string LeaveType, int Year, int TotalDays, int UsedDays, int AvailableDays);
}
