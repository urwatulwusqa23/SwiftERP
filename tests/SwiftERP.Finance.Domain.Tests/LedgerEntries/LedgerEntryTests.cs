using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.SharedKernel;

namespace SwiftERP.Finance.Domain.Tests.LedgerEntries;

public class LedgerEntryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveAmount_Throws(decimal amount)
    {
        Assert.Throws<DomainException>(() =>
            new LedgerEntry(LedgerEntryType.SaleRevenue, amount, "Test", Guid.NewGuid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_Throws(string description)
    {
        Assert.Throws<DomainException>(() =>
            new LedgerEntry(LedgerEntryType.SaleRevenue, 100m, description, Guid.NewGuid()));
    }

    [Fact]
    public void SignedAmount_ForSaleRevenue_IsPositive()
    {
        var entry = new LedgerEntry(LedgerEntryType.SaleRevenue, 100m, "Sale", Guid.NewGuid());

        Assert.Equal(100m, entry.SignedAmount);
    }

    [Theory]
    [InlineData(LedgerEntryType.PurchaseExpense)]
    [InlineData(LedgerEntryType.PayrollExpense)]
    public void SignedAmount_ForExpenseTypes_IsNegative(LedgerEntryType type)
    {
        var entry = new LedgerEntry(type, 100m, "Expense", Guid.NewGuid());

        Assert.Equal(-100m, entry.SignedAmount);
    }
}
