using MediatR;
using SwiftERP.HR.Domain.Attendance;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Attendance.ClockOut;

public class ClockOutCommandHandler(
    IAttendanceRepository attendanceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ClockOutCommand, Result>
{
    public async Task<Result> Handle(ClockOutCommand request, CancellationToken cancellationToken)
    {
        var openRecord = await attendanceRepository.GetOpenRecordAsync(request.EmployeeId, cancellationToken);
        if (openRecord is null)
            return Result.Failure("Employee is not currently clocked in.");

        openRecord.ClockOut(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
