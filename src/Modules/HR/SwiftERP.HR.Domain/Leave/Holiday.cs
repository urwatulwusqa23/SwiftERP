using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Leave;

public class Holiday : Entity
{
    public DateOnly Date { get; private set; }
    public string Name { get; private set; } = default!;

    private Holiday()
    {
    }

    public Holiday(DateOnly date, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Holiday name cannot be empty.");

        Date = date;
        Name = name;
    }
}
