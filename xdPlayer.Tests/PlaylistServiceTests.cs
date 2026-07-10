using Moq;
using xdPlayer.Application.Models;
using xdPlayer.Application.Services;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Tests.Helpers;

namespace xdPlayer.Tests;

public class PlaylistServiceTests
{
    private static (PlaylistService service, Mock<IUnitOfWork> uow, Mock<IPlaylistRepository> playlists) CreateService()
    {
        var uow = new Mock<IUnitOfWork>();
        var playlists = new Mock<IPlaylistRepository>();

        uow.SetupGet(u => u.Playlists).Returns(playlists.Object);
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var scopeFactory = TestServiceScopeFactory.Create(uow.Object);
        var service = new PlaylistService(scopeFactory);

        return (service, uow, playlists);
    }

    [Fact]
    public async Task AddTrackAsync_AssignsPositionAsMaxPlusOne_WhenGapsExist()
    {
        var (service, uow, playlists) = CreateService();

        // Simulating the playlist, where the middle track is deleted — only positions 0 і 2 are left
        var playlist = new Playlist
        {
            Id = 1,
            PlaylistTracks =
            [
                new PlaylistTrack { PlaylistId = 1, TrackId = 10, Position = 0 },
                new PlaylistTrack { PlaylistId = 1, TrackId = 30, Position = 2 },
            ]
        };

        playlists.Setup(p => p.GetWithTracksAsync(1)).ReturnsAsync(playlist);

        await service.AddTrackAsync(1, 40);

        var addedTrack = playlist.PlaylistTracks.FirstOrDefault(pt => pt.TrackId == 40);

        Assert.NotNull(addedTrack);
        Assert.Equal(3, addedTrack!.Position);
    }

    [Fact]
    public async Task AddTrackAsync_DoesNotAddDuplicate_WhenTrackAlreadyInPlaylist()
    {
        var (service, uow, playlists) = CreateService();

        var playlist = new Playlist
        {
            Id = 1,
            PlaylistTracks =
            [
                new PlaylistTrack { PlaylistId = 1, TrackId = 10, Position = 0 },
            ]
        };

        playlists.Setup(p => p.GetWithTracksAsync(1)).ReturnsAsync(playlist);

        await service.AddTrackAsync(1, 10);

        Assert.Single(playlist.PlaylistTracks);
        uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddTrackAsync_FirstTrack_GetsPositionZero()
    {
        var (service, uow, playlists) = CreateService();

        var playlist = new Playlist { Id = 1, PlaylistTracks = [] };

        playlists.Setup(p => p.GetWithTracksAsync(1)).ReturnsAsync(playlist);

        await service.AddTrackAsync(1, 99);

        var addedTrack = playlist.PlaylistTracks.First();
        Assert.Equal(0, addedTrack.Position);
    }

    [Fact]
    public async Task AddTrackAsync_ReturnsEarly_WhenPlaylistNotFound()
    {
        var (service, uow, playlists) = CreateService();

        playlists.Setup(p => p.GetWithTracksAsync(1)).ReturnsAsync((Playlist?)null);

        await service.AddTrackAsync(1, 10);

        uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveTrackAsync_RemovesCorrectTrack()
    {
        var (service, uow, playlists) = CreateService();

        var playlist = new Playlist
        {
            Id = 1,
            PlaylistTracks =
            [
                new PlaylistTrack { PlaylistId = 1, TrackId = 10, Position = 0 },
                new PlaylistTrack { PlaylistId = 1, TrackId = 20, Position = 1 },
            ]
        };

        playlists.Setup(p => p.GetWithTracksAsync(1)).ReturnsAsync(playlist);

        await service.RemoveTrackAsync(1, 10);

        Assert.Single(playlist.PlaylistTracks);
        Assert.Equal(20, playlist.PlaylistTracks.First().TrackId);
    }
}