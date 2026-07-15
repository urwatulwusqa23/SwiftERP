using MediatR;

namespace SwiftERP.HR.Application.Employees.GetEmployeeDocuments;

public record DocumentDownloadDto(string FileName, string ContentType, string StoragePath);

public record GetEmployeeDocumentByIdQuery(Guid DocumentId) : IRequest<DocumentDownloadDto?>;
