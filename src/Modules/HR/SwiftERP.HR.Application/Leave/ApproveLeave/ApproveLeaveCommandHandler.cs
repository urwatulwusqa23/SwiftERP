using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.ApproveLeave;

public class ApproveLeaveCommandHandler(
    ILeaveRequestRepository leaveRequestRepository,
    ILeaveBalanceRepository leaveBalanceRepository,
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure($"Leave request '{request.LeaveRequestId}' was not found.");

        if (!request.ApproverHasFullHrAccess)
        {
            var employee = await employeeRepository.GetByIdAsync(leaveRequest.EmployeeId, cancellationToken);
            if (employee?.ManagerId != request.ApproverEmployeeId)
                return Result.Failure("Only this employee's manager or an HR admin can approve their leave.");
        }

        var balance = await leaveBalanceRepository.GetAsync(
            leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.StartDate.Year, cancellationToken);
        if (balance is null)
            return Result.Failure("No leave balance record exists for this employee/type/year.");

        // The authoritative check: throws (mapped to 400 by the API's exception middleware) if
        // the balance can't cover it — e.g. two overlapping requests both approved in sequence.
        balance.UseDays(leaveRequest.TotalDays);
        leaveRequest.Approve();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
