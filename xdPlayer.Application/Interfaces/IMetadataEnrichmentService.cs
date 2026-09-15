using System;

namespace xdPlayer.Application.Interfaces;

public interface IMetadataEnrichmentService
{
    void EnqueueForEnrichment(int trackId);
    Task EnrichMissingOnStartupAsync();
    event EventHandler<int>? TrackEnriched;
}