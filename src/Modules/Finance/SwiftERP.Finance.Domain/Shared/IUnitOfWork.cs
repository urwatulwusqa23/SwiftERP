namespace SwiftERP.Finance.Domain.Shared;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
