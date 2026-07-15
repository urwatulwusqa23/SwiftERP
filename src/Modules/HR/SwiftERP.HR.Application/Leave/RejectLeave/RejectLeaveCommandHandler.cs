using MediatR;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.RejectLeave;

public class RejectLeaveCommandHandler(
    ILeaveRequestRepository leaveRequestRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest is null)
            return Result.Failure($"Leave request '{request.LeaveRequestId}' was not found.");

        leaveRequest.Reject(request.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
