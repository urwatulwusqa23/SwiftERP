namespace SwiftERP.HR.Application.Abstractions;

/// <summary>
/// Storage port for uploaded employee documents. The local-disk implementation lives in
/// HR.Infrastructure; swapping to cloud blob storage later only means a new implementation
/// of this interface, not touching Application or Domain.
/// </summary>
public interface IDocumentStorage
{
    Task<string> SaveAsync(Guid employeeId, string fileName, Stream content, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
