using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class Track
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public int DurationSeconds { get; set; }
    public string? CoverImagePath { get; set; }
    public string? MusicBrainzId { get; set; }
    public int PlayCount { get; set; }
    public long TotalListenedSeconds { get; set; }
    public bool IsLiked { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastPlayedAt { get; set; }

    // Navigation
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = [];
    public ICollection<TrackTag> TrackTags { get; set; } = [];
    public ICollection<ListeningSession> ListeningSessions { get; set; } = [];
}