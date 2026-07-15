using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.GetLeaveBalances;

public record LeaveBalanceDto(LeaveType LeaveType, int Year, int TotalDays, int UsedDays, int AvailableDays);
