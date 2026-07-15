using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Payroll;

public record PayrollRunPostedEvent(Guid PayrollRunId, int Year, int Month, decimal Total) : IDomainEvent;
