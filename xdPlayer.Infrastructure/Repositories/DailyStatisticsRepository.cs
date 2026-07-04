using Microsoft.EntityFrameworkCore;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class DailyStatisticsRepository : IDailyStatisticsRepository
{
    private readonly AppDbContext _db;

    public DailyStatisticsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DailyStatistics?> GetByDateAsync(DateOnly date)
        => await _db.DailyStatistics.FirstOrDefaultAsync(d => d.Date == date);

    public async Task<DateOnly?> GetLastRecordedDateAsync()
    {
        var latest = await _db.DailyStatistics
            .OrderByDescending(d => d.Date)
            .FirstOrDefaultAsync();
        return latest?.Date;
    }

    public async Task AddAsync(DailyStatistics stats)
        => await _db.DailyStatistics.AddAsync(stats);
}