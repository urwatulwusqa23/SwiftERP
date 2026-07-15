using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Attendance.ClockOut;

public record ClockOutCommand(Guid EmployeeId) : IRequest<Result>;
