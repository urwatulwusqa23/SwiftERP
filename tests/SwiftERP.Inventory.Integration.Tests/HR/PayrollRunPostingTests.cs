using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Application.Employees.HireEmployee;
using SwiftERP.HR.Application.Payroll.CreatePayrollRun;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.HR;

/// <summary>
/// Proves the second cross-module transaction in the project: posting a payroll run marks it
/// Posted and records a Finance ledger expense atomically — same pattern as ConfirmSaleOrder
/// (SwiftERP.Inventory.Integration.Tests.Sales.SaleOrderConfirmationTests), different module pair.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class PayrollRunPostingTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private SwiftErpApiFactory _factory = default!;
    private HttpClient _client = default!;

    public Task InitializeAsync()
    {
        _factory = new SwiftErpApiFactory(fixture.ConnectionString, fixture.RedisConnectionString);
        _client = _factory.CreateAuthenticatedClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<Guid> HireEmployeeAsync(decimal monthlySalary)
    {
        var command = new HireEmployeeCommand(
            $"Employee {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", monthlySalary, new DateOnly(2026, 1, 1));

        var response = await _client.PostAsJsonAsync("/api/v1/hr/employees", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task PostPayrollRun_WithActiveEmployees_PostsRunAndLedgerEntryTogether()
    {
        await HireEmployeeAsync(3000m);
        await HireEmployeeAsync(4500m);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/payroll-runs", new CreatePayrollRunCommand(2026, 7));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        await using var hrContext = fixture.CreateHrDbContext();
        var expectedTotal = await hrContext.PayrollRuns
            .Where(p => p.Id == created!.Id)
            .SelectMany(p => p.Lines)
            .SumAsync(l => l.Amount);

        var postResponse = await _client.PostAsync($"/api/v1/hr/payroll-runs/{created!.Id}/post", null);

        Assert.Equal(HttpStatusCode.NoContent, postResponse.StatusCode);

        await using var financeContext = fixture.CreateFinanceDbContext();
        var ledgerEntry = await financeContext.LedgerEntries.SingleAsync(e => e.ReferenceId == created.Id);
        Assert.Equal(expectedTotal, ledgerEntry.Amount);
    }

    [Fact]
    public async Task PostPayrollRun_WhenAlreadyPosted_FailsAndDoesNotDuplicateLedgerEntry()
    {
        await HireEmployeeAsync(2000m);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/payroll-runs", new CreatePayrollRunCommand(2026, 8));
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var firstPost = await _client.PostAsync($"/api/v1/hr/payroll-runs/{created!.Id}/post", null);
        Assert.Equal(HttpStatusCode.NoContent, firstPost.StatusCode);

        var secondPost = await _client.PostAsync($"/api/v1/hr/payroll-runs/{created.Id}/post", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondPost.StatusCode);

        await using var financeContext = fixture.CreateFinanceDbContext();
        var ledgerEntries = await financeContext.LedgerEntries
            .Where(e => e.ReferenceId == created.Id)
            .ToListAsync();
        Assert.Single(ledgerEntries);
    }

    private record CreatedResponse(Guid Id);
}
