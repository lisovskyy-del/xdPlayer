using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag> CreateAsync(string name, string color = "#1A56DB");
    Task DeleteAsync(int tagId);

    Task AddTagToTrackAsync(int trackId, int tagId);
    Task RemoveTagFromTrackAsync(int trackId, int tagId);
    Task<IEnumerable<Tag>> GetTagsForTrackAsync(int trackId);
}