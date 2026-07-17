using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.ApproveLeave;

// ApproverEmployeeId/ApproverHasFullHrAccess let the handler enforce "must be the requester's
// manager, or hold HR:Full" without the Application layer reaching into HTTP claims itself.
public record ApproveLeaveCommand(Guid LeaveRequestId, Guid ApproverEmployeeId, bool ApproverHasFullHrAccess) : IRequest<Result>;
