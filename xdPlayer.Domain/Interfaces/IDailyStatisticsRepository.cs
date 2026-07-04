using System;
using System.Collections.Generic;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface IDailyStatisticsRepository
{
    Task<DailyStatistics?> GetByDateAsync(DateOnly date);
    Task<DateOnly?> GetLastRecordedDateAsync();
    Task AddAsync(DailyStatistics stats);
}