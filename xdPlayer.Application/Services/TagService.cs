using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Services;

public class TagService : ITagService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TagService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Tags.GetAllAsync();
    }

    public async Task<Tag> CreateAsync(string name, string color = "#1A56DB")
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existing = await uow.Tags.GetByNameAsync(name);
        if (existing != null) return existing;

        var tag = new Tag { Name = name, Color = color };
        await uow.Tags.AddAsync(tag);
        await uow.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteAsync(int tagId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var tag = await uow.Tags.GetByIdAsync(tagId);
        if (tag == null) return;

        await uow.Tags.DeleteAsync(tag);
        await uow.SaveChangesAsync();
    }

    public async Task AddTagToTrackAsync(int trackId, int tagId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await uow.Tags.AddTagToTrackAsync(trackId, tagId);
    }

    public async Task RemoveTagFromTrackAsync(int trackId, int tagId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await uow.Tags.RemoveTagFromTrackAsync(trackId, tagId);
    }

    public async Task<IEnumerable<Tag>> GetTagsForTrackAsync(int trackId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Tags.GetTagsForTrackAsync(trackId);
    }
}