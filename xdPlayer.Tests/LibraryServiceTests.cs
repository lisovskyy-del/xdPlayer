using Microsoft.Extensions.DependencyInjection;
using Moq;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Models;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Tests.Helpers;

namespace xdPlayer.Tests;

public class LibraryServiceTests
{
    private static (LibraryService service, Mock<IUnitOfWork> uow, Mock<ITrackRepository> tracks, Mock<IMetadataReader> metadata, Mock<IMetadataEnrichmentService> enrichment)
        CreateService()
    {
        var uow = new Mock<IUnitOfWork>();
        var tracks = new Mock<ITrackRepository>();
        var userProfiles = new Mock<IRepository<UserProfile>>();
        var metadata = new Mock<IMetadataReader>();
        var enrichment = new Mock<IMetadataEnrichmentService>();

        uow.SetupGet(u => u.Tracks).Returns(tracks.Object);
        uow.SetupGet(u => u.UserProfiles).Returns(userProfiles.Object);
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        userProfiles.Setup(p => p.GetAllAsync())
            .ReturnsAsync(new List<UserProfile> { new() { MusicBrainzEnabled = false } });

        var scopeFactory = TestServiceScopeFactory.Create(uow.Object);
        var service = new LibraryService(scopeFactory, metadata.Object, enrichment.Object);

        return (service, uow, tracks, metadata, enrichment);
    }

    [Fact]
    public async Task AddFileAsync_EnqueuesEnrichment_WhenEnabledAndCoverMissing()
    {
        var (service, uow, tracks, metadata, enrichment) = CreateService();

        var userProfiles = new Mock<IRepository<UserProfile>>();
        userProfiles.Setup(p => p.GetAllAsync())
            .ReturnsAsync(new List<UserProfile> { new() { MusicBrainzEnabled = true } });
        uow.SetupGet(u => u.UserProfiles).Returns(userProfiles.Object);

        tracks.Setup(t => t.GetByFilePathAsync(It.IsAny<string>())).ReturnsAsync((Track?)null);
        metadata.Setup(m => m.ReadMetadata(It.IsAny<string>()))
            .Returns(new Track { Id = 7, Title = "New Track", Genre = "Rock", MusicBrainzId = "abc", CoverImagePath = null });

        await service.AddFileAsync("C:\\music\\new.mp3");

        enrichment.Verify(e => e.EnqueueForEnrichment(7), Times.Once);
    }

    [Fact]
    public async Task AddFileAsync_ReturnsExisting_WhenFilePathAlreadyExists()
    {
        var (service, uow, tracks, metadata, enrichment) = CreateService();

        var existing = new Track { Id = 5, FilePath = "C:\\music\\track.mp3", Title = "Existing" };
        tracks.Setup(t => t.GetByFilePathAsync("C:\\music\\track.mp3")).ReturnsAsync(existing);

        var result = await service.AddFileAsync("C:\\music\\track.mp3");

        Assert.Equal(5, result.Id);
        tracks.Verify(t => t.AddAsync(It.IsAny<Track>()), Times.Never);
        metadata.Verify(m => m.ReadMetadata(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddFileAsync_UsesPointQuery_NotFullTableScan()
    {
        var (service, uow, tracks, metadata, enrichment) = CreateService();

        tracks.Setup(t => t.GetByFilePathAsync(It.IsAny<string>())).ReturnsAsync((Track?)null);

        var newTrack = new Track { Title = "New Track", Genre = "Rock", MusicBrainzId = "abc" };
        metadata.Setup(m => m.ReadMetadata(It.IsAny<string>())).Returns(newTrack);

        await service.AddFileAsync("C:\\music\\new.mp3");

        tracks.Verify(t => t.GetAllAsync(), Times.Never);
        tracks.Verify(t => t.GetByFilePathAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddFileAsync_DoesNotEnqueueEnrichment_WhenMusicBrainzDisabled()
    {
        var (service, uow, tracks, metadata, enrichment) = CreateService();

        tracks.Setup(t => t.GetByFilePathAsync(It.IsAny<string>())).ReturnsAsync((Track?)null);
        metadata.Setup(m => m.ReadMetadata(It.IsAny<string>()))
            .Returns(new Track { Title = "New Track" });

        await service.AddFileAsync("C:\\music\\new.mp3");

        enrichment.Verify(e => e.EnqueueForEnrichment(It.IsAny<int>()), Times.Never);
    }
}