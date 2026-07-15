using SwiftERP.Inventory.Domain.Products;
using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.Tests.Products;

public class ProductTests
{
    private static Product CreateProduct(int reorderThreshold = 5, int initialQuantity = 20) =>
        new("SKU-001", "Test Widget", reorderThreshold, Guid.NewGuid(), initialQuantity);

    [Fact]
    public void Constructor_WithValidArgs_SetsQuantityOnHand()
    {
        var product = CreateProduct(initialQuantity: 20);

        Assert.Equal(20, product.QuantityOnHand);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptySku_Throws(string sku)
    {
        Assert.Throws<DomainException>(() => new Product(sku, "Name", 5, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithNegativeReorderThreshold_Throws()
    {
        Assert.Throws<DomainException>(() => new Product("SKU-001", "Name", -1, Guid.NewGuid()));
    }

    [Fact]
    public void DecrementStock_WithSufficientQuantity_ReducesQuantityOnHand()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 20);

        product.DecrementStock(15);

        Assert.Equal(5, product.QuantityOnHand);
    }

    [Fact]
    public void DecrementStock_BelowZero_ThrowsAndLeavesQuantityUnchanged()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 10);

        var ex = Assert.Throws<DomainException>(() => product.DecrementStock(11));

        Assert.Contains("Cannot decrement", ex.Message);
        Assert.Equal(10, product.QuantityOnHand);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DecrementStock_WithNonPositiveQuantity_Throws(int quantity)
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.DecrementStock(quantity));
    }

    [Fact]
    public void DecrementStock_CrossingReorderThreshold_RaisesLowStockEvent()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 10);

        product.DecrementStock(6);

        var domainEvent = Assert.Single(product.DomainEvents);
        var lowStockEvent = Assert.IsType<ProductLowStockEvent>(domainEvent);
        Assert.Equal(4, lowStockEvent.QuantityOnHand);
    }

    [Fact]
    public void DecrementStock_StayingAboveReorderThreshold_DoesNotRaiseLowStockEvent()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 20);

        product.DecrementStock(5);

        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void ReceiveStock_WithPositiveQuantity_IncreasesQuantityOnHand()
    {
        var product = CreateProduct(initialQuantity: 10);

        product.ReceiveStock(5);

        Assert.Equal(15, product.QuantityOnHand);
    }

    [Fact]
    public void ReceiveStock_WithNonPositiveQuantity_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.ReceiveStock(0));
    }

    [Fact]
    public void AdjustStock_WithNegativeQuantity_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.AdjustStock(-1));
    }

    [Fact]
    public void IsLowStock_WhenQuantityAtOrBelowThreshold_ReturnsTrue()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 5);

        Assert.True(product.IsLowStock);
    }

    [Fact]
    public void IsLowStock_WhenQuantityAboveThreshold_ReturnsFalse()
    {
        var product = CreateProduct(reorderThreshold: 5, initialQuantity: 6);

        Assert.False(product.IsLowStock);
    }
}
