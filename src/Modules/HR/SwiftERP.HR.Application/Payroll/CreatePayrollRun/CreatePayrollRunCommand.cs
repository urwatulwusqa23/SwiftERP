using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Payroll.CreatePayrollRun;

public record CreatePayrollRunCommand(int Year, int Month) : IRequest<Result<Guid>>;
