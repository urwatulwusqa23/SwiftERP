using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Application.Leave.Holidays;

public record CreateHolidayCommand(DateOnly Date, string Name) : IRequest<Result<Guid>>;
