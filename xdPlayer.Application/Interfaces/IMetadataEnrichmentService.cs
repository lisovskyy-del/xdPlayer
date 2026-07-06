using System;

namespace xdPlayer.Application.Interfaces;

public interface IMetadataEnrichmentService
{
    void EnqueueForEnrichment(int trackId);
    event EventHandler<int>? TrackEnriched;
}