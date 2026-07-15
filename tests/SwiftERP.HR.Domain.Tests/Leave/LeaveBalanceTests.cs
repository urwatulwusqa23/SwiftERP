using SwiftERP.HR.Domain.Leave;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Leave;

public class LeaveBalanceTests
{
    [Fact]
    public void Constructor_WithoutExplicitTotal_UsesDefaultEntitlement()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), LeaveType.Annual, 2026);

        Assert.Equal(LeaveBalance.DefaultEntitlements[LeaveType.Annual], balance.TotalDays);
        Assert.Equal(0, balance.UsedDays);
        Assert.Equal(balance.TotalDays, balance.AvailableDays);
    }

    [Fact]
    public void UseDays_WithinAvailable_DecreasesAvailableDays()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), LeaveType.Sick, 2026, totalDays: 10);

        balance.UseDays(4);

        Assert.Equal(4, balance.UsedDays);
        Assert.Equal(6, balance.AvailableDays);
    }

    [Fact]
    public void UseDays_ExceedingAvailable_Throws()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), LeaveType.Sick, 2026, totalDays: 5);

        var ex = Assert.Throws<DomainException>(() => balance.UseDays(6));
        Assert.Contains("only 5 available", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UseDays_WithNonPositiveDays_Throws(int days)
    {
        var balance = new LeaveBalance(Guid.NewGuid(), LeaveType.Casual, 2026);

        Assert.Throws<DomainException>(() => balance.UseDays(days));
    }

    [Fact]
    public void RefundDays_ReducesUsedDaysWithoutGoingNegative()
    {
        var balance = new LeaveBalance(Guid.NewGuid(), LeaveType.Casual, 2026, totalDays: 10);
        balance.UseDays(3);

        balance.RefundDays(10);

        Assert.Equal(0, balance.UsedDays);
    }
}
