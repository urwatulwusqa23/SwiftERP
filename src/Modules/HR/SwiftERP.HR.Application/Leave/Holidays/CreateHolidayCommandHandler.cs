using MediatR;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.Holidays;

public class CreateHolidayCommandHandler(
    IHolidayRepository holidayRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateHolidayCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = new Holiday(request.Date, request.Name);

        holidayRepository.Add(holiday);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(holiday.Id);
    }
}
