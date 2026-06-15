using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class Track
{
    public int Id { get; set; }

    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Artist { get; set; }

    [MaxLength(300)]
    public string? Album { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int? Year { get; set; }
    public int DurationSeconds { get; set; }

    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    [MaxLength(36)]
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