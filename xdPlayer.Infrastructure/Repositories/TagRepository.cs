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
            .FirstOrDefaultAsync(t => t.Name == name);
}