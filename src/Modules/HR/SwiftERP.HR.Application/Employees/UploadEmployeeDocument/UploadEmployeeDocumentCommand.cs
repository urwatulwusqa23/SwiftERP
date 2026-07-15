using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.UploadEmployeeDocument;

public record UploadEmployeeDocumentCommand(
    Guid EmployeeId,
    EmployeeDocumentType DocumentType,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content) : IRequest<Result<Guid>>;
