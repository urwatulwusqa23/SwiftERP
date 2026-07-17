using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetEmployeeDocuments;

public class GetEmployeeDocumentByIdQueryHandler(IEmployeeDocumentRepository documentRepository)
    : IRequestHandler<GetEmployeeDocumentByIdQuery, DocumentDownloadDto?>
{
    public async Task<DocumentDownloadDto?> Handle(GetEmployeeDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        return document is null
            ? null
            : new DocumentDownloadDto(document.EmployeeId, document.FileName, document.ContentType, document.StoragePath);
    }
}
