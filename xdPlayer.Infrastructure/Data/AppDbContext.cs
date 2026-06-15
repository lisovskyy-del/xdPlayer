using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Infrastructure.Data;
// The main bridge between sql and c#

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<TrackTag> TrackTags => Set<TrackTag>();
    public DbSet<ListeningSession> ListeningSessions => Set<ListeningSession>();
    public DbSet<DailyStatistics> DailyStatistics => Set<DailyStatistics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(pt => pt.PlaylistId);

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany(t => t.PlaylistTracks)
            .HasForeignKey(pt => pt.TrackId);

        modelBuilder.Entity<TrackTag>()
            .HasKey(tt => new { tt.TrackId, tt.TagId });

        modelBuilder.Entity<TrackTag>()
            .HasOne(tt => tt.Track)
            .WithMany(t => t.TrackTags)
            .HasForeignKey(tt => tt.TrackId);

        modelBuilder.Entity<DailyStatistics>()
            .HasIndex(d => d.Date)
            .IsUnique();

        modelBuilder.Entity<Track>()
            .HasIndex(t => t.FilePath)
            .IsUnique();
    }
}