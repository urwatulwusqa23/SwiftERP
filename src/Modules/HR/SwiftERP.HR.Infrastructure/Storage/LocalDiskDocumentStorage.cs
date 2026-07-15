using Microsoft.Extensions.Configuration;
using SwiftERP.HR.Application.Abstractions;

namespace SwiftERP.HR.Infrastructure.Storage;

/// <summary>
/// Stores uploaded documents on the local filesystem (a Docker volume in practice), under
/// {root}/{employeeId}/{guid}_{originalFileName}. The DB only ever stores the relative path
/// returned here, never the file content — see EmployeeDocument.StoragePath.
/// </summary>
public class LocalDiskDocumentStorage : IDocumentStorage
{
    private readonly string _rootPath;

    public LocalDiskDocumentStorage(IConfiguration configuration)
    {
        _rootPath = configuration["DocumentStorage:RootPath"] ?? "App_Data/documents";
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Guid employeeId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(fileName);
        var relativePath = Path.Combine(employeeId.ToString(), $"{Guid.NewGuid():N}_{safeFileName}");
        var fullPath = Path.Combine(_rootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativePath.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored document could not be found on disk.", fullPath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }
}
