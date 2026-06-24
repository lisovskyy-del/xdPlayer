using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface IListeningSessionRepository
{
    Task<ListeningSession?> GetByIdAsync(long id);
    Task<ListeningSession?> GetActiveSessionAsync(int trackId);
    Task AddAsync(ListeningSession session);
    Task SaveAsync();
}