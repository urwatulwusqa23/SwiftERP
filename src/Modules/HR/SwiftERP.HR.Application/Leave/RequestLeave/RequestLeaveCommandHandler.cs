using MediatR;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.RequestLeave;

public class RequestLeaveCommandHandler(
    IEmployeeRepository employeeRepository,
    ILeaveRequestRepository leaveRequestRepository,
    ILeaveBalanceRepository leaveBalanceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RequestLeaveCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RequestLeaveCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<Guid>($"Employee '{request.EmployeeId}' was not found.");

        var leaveRequest = new LeaveRequest(
            request.EmployeeId, request.LeaveType, request.StartDate, request.EndDate, request.Reason);

        // Early, non-authoritative check so an obviously-overdrawn request fails fast with a
        // useful message — the balance is only actually deducted (and re-checked) on approval.
        var balance = await GetOrCreateBalanceAsync(request.EmployeeId, request.LeaveType, request.StartDate.Year, cancellationToken);
        if (leaveRequest.TotalDays > balance.AvailableDays)
            return Result.Failure<Guid>(
                $"Requested {leaveRequest.TotalDays} day(s) of {request.LeaveType} leave, but only {balance.AvailableDays} available.");

        leaveRequestRepository.Add(leaveRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(leaveRequest.Id);
    }

    private async Task<LeaveBalance> GetOrCreateBalanceAsync(
        Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken)
    {
        var existing = await leaveBalanceRepository.GetAsync(employeeId, leaveType, year, cancellationToken);
        if (existing is not null)
            return existing;

        var created = new LeaveBalance(employeeId, leaveType, year);
        leaveBalanceRepository.Add(created);
        return created;
    }
}
