using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;
// DB Connection between table and Application

public class TrackTag
{
    public int TrackId { get; set; }
    public int TagId { get; set; }

    // Navigation, by default set as null
    public Track Track { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
