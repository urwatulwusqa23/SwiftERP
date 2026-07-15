using Microsoft.EntityFrameworkCore;
using SwiftERP.HR.Domain.Leave;

namespace SwiftERP.HR.Infrastructure.Persistence.Repositories;

public class HolidayRepository(HrDbContext dbContext) : IHolidayRepository
{
    public Task<List<Holiday>> GetForYearAsync(int year, CancellationToken cancellationToken) =>
        dbContext.Holidays.Where(h => h.Date.Year == year).ToListAsync(cancellationToken);

    public void Add(Holiday holiday) => dbContext.Holidays.Add(holiday);
}
