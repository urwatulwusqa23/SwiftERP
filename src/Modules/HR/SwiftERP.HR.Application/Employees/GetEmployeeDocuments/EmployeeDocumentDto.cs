using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetEmployeeDocuments;

public record EmployeeDocumentDto(
    Guid Id,
    EmployeeDocumentType DocumentType,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAtUtc);
