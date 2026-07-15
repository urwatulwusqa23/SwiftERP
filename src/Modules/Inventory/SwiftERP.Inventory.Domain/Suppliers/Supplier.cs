using SwiftERP.SharedKernel;

namespace SwiftERP.Inventory.Domain.Suppliers;

public class Supplier : Entity
{
    public string Name { get; private set; } = default!;
    public string ContactEmail { get; private set; } = default!;

    private Supplier()
    {
    }

    public Supplier(string name, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name cannot be empty.");
        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("Supplier contact email cannot be empty.");

        Name = name;
        ContactEmail = contactEmail;
    }
}
