using Microsoft.EntityFrameworkCore;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context) { }

    public async Task<Tag?> GetByNameAsync(string name) =>
        await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

    public async Task<IEnumerable<Tag>> GetTagsForTrackAsync(int trackId) =>
        await _context.TrackTags
            .Where(tt => tt.TrackId == trackId)
            .Select(tt => tt.Tag)
            .ToListAsync();

    public async Task AddTagToTrackAsync(int trackId, int tagId)
    {
        var exists = await _context.TrackTags
            .AnyAsync(tt => tt.TrackId == trackId && tt.TagId == tagId);

        if (exists) return;

        _context.TrackTags.Add(new TrackTag
        {
            TrackId = trackId,
            TagId = tagId,
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveTagFromTrackAsync(int trackId, int tagId)
    {
        var trackTag = await _context.TrackTags
            .FirstOrDefaultAsync(tt => tt.TrackId == trackId && tt.TagId == tagId);

        if (trackTag == null) return;

        _context.TrackTags.Remove(trackTag);
        await _context.SaveChangesAsync();
    }
}