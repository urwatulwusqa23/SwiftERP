using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Employees;

public enum EmployeeDocumentType
{
    Contract,
    IdProof,
    Certificate,
    Other
}

public class EmployeeDocument : Entity
{
    public Guid EmployeeId { get; private set; }
    public EmployeeDocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

    private EmployeeDocument()
    {
    }

    public EmployeeDocument(
        Guid employeeId,
        EmployeeDocumentType documentType,
        string fileName,
        string storagePath,
        string contentType,
        long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Document file name cannot be empty.");
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException("Document storage path cannot be empty.");
        if (sizeBytes <= 0)
            throw new DomainException("Document size must be positive.");

        EmployeeId = employeeId;
        DocumentType = documentType;
        FileName = fileName;
        StoragePath = storagePath;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        SizeBytes = sizeBytes;
        UploadedAtUtc = DateTimeOffset.UtcNow;
    }
}
