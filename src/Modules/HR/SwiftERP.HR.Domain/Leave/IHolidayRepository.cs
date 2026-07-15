namespace SwiftERP.HR.Domain.Leave;

public interface IHolidayRepository
{
    Task<List<Holiday>> GetForYearAsync(int year, CancellationToken cancellationToken);
    void Add(Holiday holiday);
}
