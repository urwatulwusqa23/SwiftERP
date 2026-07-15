using SwiftERP.HR.Domain.Leave;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Leave;

public class HolidayTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_Throws(string name)
    {
        Assert.Throws<DomainException>(() => new Holiday(new DateOnly(2026, 12, 25), name));
    }

    [Fact]
    public void Constructor_WithValidArgs_Succeeds()
    {
        var holiday = new Holiday(new DateOnly(2026, 12, 25), "Christmas Day");

        Assert.Equal("Christmas Day", holiday.Name);
    }
}
