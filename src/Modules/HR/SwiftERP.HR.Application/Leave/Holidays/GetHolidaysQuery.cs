using MediatR;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Application.Leave.Holidays;

public record HolidayDto(Guid Id, DateOnly Date, string Name);

public record GetHolidaysQuery(int Year) : IRequest<List<HolidayDto>>;
