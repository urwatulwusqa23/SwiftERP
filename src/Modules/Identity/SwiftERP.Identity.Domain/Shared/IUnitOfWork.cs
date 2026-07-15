namespace SwiftERP.Identity.Domain.Shared;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
