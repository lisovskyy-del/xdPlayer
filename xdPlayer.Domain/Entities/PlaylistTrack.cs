using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class PlaylistTrack
{
    public int PlaylistId { get; set; }
    public int TrackId { get; set; }
    public int Position { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation, by default set as null;

    public PlaylistTrack Playlist { get; set; } = null!;
    public Track Track { get; set; } = null!;
}