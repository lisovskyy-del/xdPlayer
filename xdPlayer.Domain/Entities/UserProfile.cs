using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace xdPlayer.Domain.Entities;

// Connection between DB table and Application

public class UserProfile
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string DisplayName { get; set; } = "User";

    [MaxLength(500)]
    public string? AvatarPath { get; set; }

    [MaxLength(7)]
    public string AccentColor { get; set; } = "#1A56DB";

    [MaxLength(20)]
    public string Theme { get; set; } = "Dark";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
}