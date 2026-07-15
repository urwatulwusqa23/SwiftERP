using MediatR;

namespace SwiftERP.HR.Application.Employees.GetEmployeeDocuments;

public record GetEmployeeDocumentsQuery(Guid EmployeeId) : IRequest<List<EmployeeDocumentDto>>;
