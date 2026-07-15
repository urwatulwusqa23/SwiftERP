using SwiftERP.HR.Domain.Leave;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Leave;

public class LeaveRequestTests
{
    private static LeaveRequest CreateRequest(DateOnly? start = null, DateOnly? end = null) =>
        new(Guid.NewGuid(), LeaveType.Annual, start ?? new DateOnly(2026, 7, 10), end ?? new DateOnly(2026, 7, 12), "Vacation");

    [Fact]
    public void Constructor_WithEndBeforeStart_Throws()
    {
        Assert.Throws<DomainException>(() =>
            CreateRequest(new DateOnly(2026, 7, 12), new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void Constructor_WithValidDates_StartsPending()
    {
        var request = CreateRequest();

        Assert.Equal(LeaveRequestStatus.Pending, request.Status);
    }

    [Fact]
    public void TotalDays_IsInclusiveOfBothEndpoints()
    {
        var request = CreateRequest(new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 12));

        Assert.Equal(3, request.TotalDays);
    }

    [Fact]
    public void Approve_WhenPending_TransitionsToApproved()
    {
        var request = CreateRequest();

        request.Approve();

        Assert.Equal(LeaveRequestStatus.Approved, request.Status);
        Assert.NotNull(request.DecidedAtUtc);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var request = CreateRequest();
        request.Approve();

        Assert.Throws<DomainException>(request.Approve);
    }

    [Fact]
    public void Reject_WhenPending_TransitionsToRejectedWithNote()
    {
        var request = CreateRequest();

        request.Reject("Team is short-staffed that week.");

        Assert.Equal(LeaveRequestStatus.Rejected, request.Status);
        Assert.Equal("Team is short-staffed that week.", request.DecisionNote);
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_Throws()
    {
        var request = CreateRequest();
        request.Approve();

        Assert.Throws<DomainException>(() => request.Reject(null));
    }
}
