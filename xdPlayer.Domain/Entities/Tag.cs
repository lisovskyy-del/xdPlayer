using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace xdPlayer.Domain.Entities;

// DB Connection between table and Application

public class Tag
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string Color { get; set; } = "#1A56DB";

    // Navigation
    public ICollection<TrackTag> TrackTags { get; set; } = [];
}