using SwiftERP.HR.Domain.Payroll;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Payroll;

public class PayrollRunTests
{
    private static List<PayrollRunLine> TwoLines() =>
    [
        new PayrollRunLine(Guid.NewGuid(), 3000m),
        new PayrollRunLine(Guid.NewGuid(), 4500m)
    ];

    [Fact]
    public void Constructor_WithNoLines_Throws()
    {
        Assert.Throws<DomainException>(() => new PayrollRun(2026, 7, []));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Constructor_WithInvalidMonth_Throws(int month)
    {
        Assert.Throws<DomainException>(() => new PayrollRun(2026, month, TwoLines()));
    }

    [Fact]
    public void Constructor_WithValidLines_StartsDraft()
    {
        var run = new PayrollRun(2026, 7, TwoLines());

        Assert.Equal(PayrollRunStatus.Draft, run.Status);
    }

    [Fact]
    public void Total_SumsAllLineAmounts()
    {
        var run = new PayrollRun(2026, 7, TwoLines());

        Assert.Equal(7500m, run.Total);
    }

    [Fact]
    public void Post_WhenDraft_TransitionsToPosted()
    {
        var run = new PayrollRun(2026, 7, TwoLines());

        run.Post();

        Assert.Equal(PayrollRunStatus.Posted, run.Status);
        Assert.NotNull(run.PostedAtUtc);
    }

    [Fact]
    public void Post_WhenAlreadyPosted_Throws()
    {
        var run = new PayrollRun(2026, 7, TwoLines());
        run.Post();

        Assert.Throws<DomainException>(run.Post);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PayrollRunLine_WithNonPositiveAmount_Throws(decimal amount)
    {
        Assert.Throws<DomainException>(() => new PayrollRunLine(Guid.NewGuid(), amount));
    }
}
