using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}