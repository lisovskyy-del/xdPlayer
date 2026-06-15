using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface ITrackRepository : IRepository<Track>
{
    Task<IEnumerable<Track>> SearchAsync(string query);
    Task<IEnumerable<Track>> GetLikedAsync();
    Task<IEnumerable<Track>> GetByGenreAsync(string genre);
    Task<IEnumerable<Track>> GetPagedAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
}