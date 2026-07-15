using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Payroll;

public enum PayrollRunStatus
{
    Draft,
    Posted
}

public class PayrollRun : Entity
{
    public int Year { get; private set; }
    public int Month { get; private set; }
    public PayrollRunStatus Status { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }

    private readonly List<PayrollRunLine> _lines = [];
    public IReadOnlyCollection<PayrollRunLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(l => l.Amount);

    private PayrollRun()
    {
    }

    public PayrollRun(int year, int month, IEnumerable<PayrollRunLine> lines)
    {
        if (month is < 1 or > 12)
            throw new DomainException("Payroll run month must be between 1 and 12.");

        var lineList = lines.ToList();
        if (lineList.Count == 0)
            throw new DomainException("A payroll run must have at least one line.");

        Year = year;
        Month = month;
        Status = PayrollRunStatus.Draft;
        _lines.AddRange(lineList);
    }

    public void Post()
    {
        if (Status != PayrollRunStatus.Draft)
            throw new DomainException($"Cannot post a payroll run in status '{Status}'.");

        Status = PayrollRunStatus.Posted;
        PostedAtUtc = DateTimeOffset.UtcNow;

        Raise(new PayrollRunPostedEvent(Id, Year, Month, Total));
    }
}

public class PayrollRunLine
{
    public Guid EmployeeId { get; private set; }
    public decimal Amount { get; private set; }

    private PayrollRunLine()
    {
    }

    public PayrollRunLine(Guid employeeId, decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Payroll run line amount must be positive.");

        EmployeeId = employeeId;
        Amount = amount;
    }
}
