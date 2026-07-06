namespace xdPlayer.Application.Interfaces;

public class MusicBrainzResult
{
    public string? MusicBrainzId { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public string? ReleaseMbid { get; set; }
}

public interface IMusicBrainzClient
{
    Task<MusicBrainzResult?> SearchRecordingAsync(string title, string? artist);
    Task<string?> GetCoverArtUrlAsync(string releaseMbid);
}