using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Attendance.ClockIn;

public record ClockInCommand(Guid EmployeeId) : IRequest<Result<Guid>>;
