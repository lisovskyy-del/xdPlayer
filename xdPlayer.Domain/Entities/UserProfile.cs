using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Entities;

// Connection between DB table and Application

public class UserProfile
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "User";
    public string? AvatarPath { get; set; }
    public string AccentColor { get; set; } = "#1A56DB";
    public string Theme { get; set; } = "Dark";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
}