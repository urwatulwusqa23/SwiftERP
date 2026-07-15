using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Payroll.PostPayrollRun;

public record PostPayrollRunCommand(Guid PayrollRunId) : IRequest<Result>;
