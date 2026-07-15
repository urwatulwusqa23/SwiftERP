using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.RejectLeave;

public record RejectLeaveCommand(Guid LeaveRequestId, string? Note) : IRequest<Result>;
