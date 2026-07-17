using System.Net;
using System.Net.Http.Json;
using SwiftERP.Inventory.Application.Products.CreateProduct;
using SwiftERP.Inventory.Integration.Tests.Infrastructure;

namespace SwiftERP.Inventory.Integration.Tests.Products;

[Collection(SqlServerCollection.Name)]
public class ProductEndpointsTests(SqlServerContainerFixture fixture) : IAsyncLifetime
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
    public async Task CreateProduct_ThenAdjustStock_ReflectsInLowStockQuery()
    {
        var command = new CreateProductCommand(
            $"SKU-{Guid.NewGuid():N}", "End-to-End Widget", ReorderThreshold: 10, Guid.NewGuid(), InitialQuantity: 20);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/inventory/products", command);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(created);

        var adjustResponse = await _client.PutAsJsonAsync(
            $"/api/v1/inventory/products/{created!.Id}/stock", new { NewQuantity = 3 });
        Assert.Equal(HttpStatusCode.NoContent, adjustResponse.StatusCode);

        var lowStockResponse = await _client.GetAsync("/api/v1/inventory/products/low-stock");
        Assert.Equal(HttpStatusCode.OK, lowStockResponse.StatusCode);

        var lowStock = await lowStockResponse.Content.ReadFromJsonAsync<List<LowStockProductResponse>>();
        Assert.Contains(lowStock!, p => p.Id == created.Id);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsBadRequest()
    {
        var sku = $"SKU-{Guid.NewGuid():N}";
        var command = new CreateProductCommand(sku, "First Widget", 5, Guid.NewGuid(), 10);

        var first = await _client.PostAsJsonAsync("/api/v1/inventory/products", command);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = new CreateProductCommand(sku, "Second Widget", 5, Guid.NewGuid(), 10);
        var second = await _client.PostAsJsonAsync("/api/v1/inventory/products", duplicate);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCommand_ReturnsBadRequestFromValidation()
    {
        var command = new CreateProductCommand("", "", ReorderThreshold: -1, Guid.Empty, InitialQuantity: -5);

        var response = await _client.PostAsJsonAsync("/api/v1/inventory/products", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record CreatedResponse(Guid Id);
    private record LowStockProductResponse(Guid Id, string Sku, string Name, int QuantityOnHand, int ReorderThreshold);
}
