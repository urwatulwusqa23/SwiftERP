using MediatR;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.HR.Application.Employees.GetEmployeeDocuments;

public class GetEmployeeDocumentsQueryHandler(IEmployeeDocumentRepository documentRepository)
    : IRequestHandler<GetEmployeeDocumentsQuery, List<EmployeeDocumentDto>>
{
    public async Task<List<EmployeeDocumentDto>> Handle(GetEmployeeDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetForEmployeeAsync(request.EmployeeId, cancellationToken);

        return documents
            .Select(d => new EmployeeDocumentDto(d.Id, d.DocumentType, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAtUtc))
            .ToList();
    }
}
