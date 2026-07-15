using MediatR;
using SwiftERP.HR.Application.Abstractions;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Employees.UploadEmployeeDocument;

public class UploadEmployeeDocumentCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeDocumentRepository documentRepository,
    IDocumentStorage documentStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadEmployeeDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadEmployeeDocumentCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<Guid>($"Employee '{request.EmployeeId}' was not found.");

        var storagePath = await documentStorage.SaveAsync(
            request.EmployeeId, request.FileName, request.Content, cancellationToken);

        var document = new EmployeeDocument(
            request.EmployeeId,
            request.DocumentType,
            request.FileName,
            storagePath,
            request.ContentType,
            request.SizeBytes);

        documentRepository.Add(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(document.Id);
    }
}
