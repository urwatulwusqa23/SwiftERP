using MediatR;
using SwiftERP.HR.Domain.Attendance;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Attendance.ClockIn;

public class ClockInCommandHandler(
    IEmployeeRepository employeeRepository,
    IAttendanceRepository attendanceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClockInCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ClockInCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<Guid>($"Employee '{request.EmployeeId}' was not found.");

        var openRecord = await attendanceRepository.GetOpenRecordAsync(request.EmployeeId, cancellationToken);
        if (openRecord is not null)
            return Result.Failure<Guid>("Employee is already clocked in — clock out first.");

        var now = DateTimeOffset.UtcNow;
        var record = new AttendanceRecord(request.EmployeeId, DateOnly.FromDateTime(now.UtcDateTime), now);

        attendanceRepository.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(record.Id);
    }
}
