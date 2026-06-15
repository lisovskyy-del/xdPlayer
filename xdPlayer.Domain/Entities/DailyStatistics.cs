using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

public class DailyStatistics
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TotalListenedSeconds { get; set; }
    public int TracksPlayedCount { get; set; }
    public int UniqueTracksCount { get; set; }
    public int? TopTrackId { get; set; }

    // Navigation
    public Track? TopTrack { get; set; }
}