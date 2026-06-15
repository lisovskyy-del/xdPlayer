using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#1A56DB";

    // Navigation
    public ICollection<TrackTag> TrackTags { get; set; } = [];
}