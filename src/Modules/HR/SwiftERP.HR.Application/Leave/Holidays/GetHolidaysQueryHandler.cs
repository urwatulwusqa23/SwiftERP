using MediatR;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.Holidays;

public class GetHolidaysQueryHandler(IHolidayRepository holidayRepository)
    : IRequestHandler<GetHolidaysQuery, List<HolidayDto>>
{
    public async Task<List<HolidayDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        var holidays = await holidayRepository.GetForYearAsync(request.Year, cancellationToken);

        return holidays
            .Select(h => new HolidayDto(h.Id, h.Date, h.Name))
            .OrderBy(h => h.Date)
            .ToList();
    }
}
