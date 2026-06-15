using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = [];
}