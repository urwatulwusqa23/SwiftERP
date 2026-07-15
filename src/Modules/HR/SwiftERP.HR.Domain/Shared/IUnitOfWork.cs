namespace SwiftERP.HR.Domain.Shared;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
