using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.ApproveLeave;

public record ApproveLeaveCommand(Guid LeaveRequestId) : IRequest<Result>;
