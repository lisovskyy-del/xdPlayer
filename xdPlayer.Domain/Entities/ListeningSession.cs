using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;
// DB Connection between table and Application


public class ListeningSession
{
    public long Id { get; set; }
    public int TrackId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int ListenedSeconds { get; set; }
    public bool CompletedFully { get; set; }

    // Navigation, set as null by default
    public Track Track { get; set; } = null!;
}
