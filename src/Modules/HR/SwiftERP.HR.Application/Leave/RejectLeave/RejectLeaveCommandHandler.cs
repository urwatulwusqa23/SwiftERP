using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.RejectLeave;

public class RejectLeaveCommandHandler(
    ILeaveRequestRepository leaveRequestRepository,
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure($"Leave request '{request.LeaveRequestId}' was not found.");

        if (!request.ApproverHasFullHrAccess)
        {
            var employee = await employeeRepository.GetByIdAsync(leaveRequest.EmployeeId, cancellationToken);
            if (employee?.ManagerId != request.ApproverEmployeeId)
                return Result.Failure("Only this employee's manager or an HR admin can reject their leave.");
        }

        leaveRequest.Reject(request.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
