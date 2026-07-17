using System.Net;
using System.Net.Http.Json;
using System.Text;
using SwiftERP.HR.Application.Employees.HireEmployee;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.HR;

/// <summary>
/// Proves the local-disk document storage roundtrip: upload a file via multipart/form-data,
/// then download it back through the API and assert the bytes match exactly.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class EmployeeDocumentTests(SqlServerContainerFixture fixture) : IAsyncLifetime
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

    [Fact]
    public async Task UploadThenDownloadDocument_ReturnsIdenticalBytes()
    {
        var hireResponse = await _client.PostAsJsonAsync(
            "/api/v1/hr/employees",
            new HireEmployeeCommand($"Doc Test {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com", 3000m, new DateOnly(2026, 1, 1)));
        hireResponse.EnsureSuccessStatusCode();
        var employee = await hireResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var originalContent = "This is a test contract document."u8.ToArray();

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(originalContent);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "contract.txt");
        form.Add(new StringContent("Contract"), "documentType");

        var uploadResponse = await _client.PostAsync($"/api/v1/hr/employees/{employee!.Id}/documents", form);
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var listResponse = await _client.GetAsync($"/api/v1/hr/employees/{employee.Id}/documents");
        var documents = await listResponse.Content.ReadFromJsonAsync<List<DocumentSummary>>();
        Assert.Contains(documents!, d => d.Id == uploaded!.Id && d.FileName == "contract.txt");

        var downloadResponse = await _client.GetAsync($"/api/v1/hr/documents/{uploaded!.Id}/download");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();

        Assert.Equal(originalContent, downloadedBytes);
    }

    private record CreatedResponse(Guid Id);
    private record DocumentSummary(Guid Id, string DocumentType, string FileName);
}
