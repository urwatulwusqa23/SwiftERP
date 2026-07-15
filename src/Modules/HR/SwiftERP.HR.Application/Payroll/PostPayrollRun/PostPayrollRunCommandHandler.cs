using MediatR;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.HR.Application.Abstractions;
using SwiftERP.HR.Domain.Payroll;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Payroll.PostPayrollRun;

/// <summary>
/// Mirrors Sales' ConfirmSaleOrder cross-module pattern: posting a payroll run must mark it Posted
/// and record a Finance ledger expense in one transaction. <see cref="IHrLedgerPort"/> is backed by
/// the same HrDbContext instance as <see cref="IPayrollRunRepository"/> (see HR.Infrastructure DI
/// wiring), so the single SaveChangesAsync call commits both changes together.
/// </summary>
public class PostPayrollRunCommandHandler(
    IPayrollRunRepository payrollRunRepository,
    IHrLedgerPort ledgerPort,
    IUnitOfWork unitOfWork) : IRequestHandler<PostPayrollRunCommand, Result>
{
    public async Task<Result> Handle(PostPayrollRunCommand request, CancellationToken cancellationToken)
    {
        var payrollRun = await payrollRunRepository.GetByIdAsync(request.PayrollRunId, cancellationToken);
        if (payrollRun is null)
            return Result.Failure($"Payroll run '{request.PayrollRunId}' was not found.");

        payrollRun.Post();

        var ledgerEntry = new LedgerEntry(
            LedgerEntryType.PayrollExpense,
            payrollRun.Total,
            $"Payroll run {payrollRun.Year}-{payrollRun.Month:D2} posted",
            payrollRun.Id);
        ledgerPort.AddEntry(ledgerEntry);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
