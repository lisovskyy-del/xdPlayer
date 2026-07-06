using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Polly;
using Polly.Retry;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.Infrastructure.Services;

public class MusicBrainzClient : IMusicBrainzClient
{
    private readonly HttpClient _http;
    private readonly ResiliencePipeline _pipeline;

    public MusicBrainzClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("xdPlayer/1.0 (personal music player)");

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential
            })
            .Build();
    }

    public async Task<string?> GetCoverArtUrlAsync(string releaseMbid)
    {
        return await _pipeline.ExecuteAsync(async _ =>
        {
            var url = $"https://coverartarchive.org/release/{releaseMbid}/front-250";

            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _http.SendAsync(request);

            return response.IsSuccessStatusCode || (int)response.StatusCode is >= 300 and < 400
                ? url
                : null;
        });
    }

    public async Task<MusicBrainzResult?> SearchRecordingAsync(string title, string? artist)
    {
        var query = artist != null
            ? $"recording:\"{title}\" AND artist:\"{artist}\""
            : $"recording:\"{title}\"";

        var url = $"recording/?query={Uri.EscapeDataString(query)}&fmt=json&limit=1";

        return await _pipeline.ExecuteAsync(async _ =>
        {
            var response = await _http.GetFromJsonAsync<MusicBrainzSearchResponse>(url);
            var recording = response?.Recordings?.FirstOrDefault();
            if (recording == null) return null;

            var release = recording.Releases?.FirstOrDefault();

            return new MusicBrainzResult
            {
                MusicBrainzId = recording.Id,
                Artist = recording.ArtistCredit?.FirstOrDefault()?.Name,
                Album = release?.Title,
                ReleaseMbid = release?.Id,
                Genre = recording.Tags?.OrderByDescending(t => t.Count).FirstOrDefault()?.Name
            };
        });
    }

    private class MusicBrainzSearchResponse
    {
        [JsonPropertyName("recordings")]
        public List<RecordingDto>? Recordings { get; set; }
    }

    private class RecordingDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("artist-credit")]
        public List<ArtistCreditDto>? ArtistCredit { get; set; }

        [JsonPropertyName("releases")]
        public List<ReleaseDto>? Releases { get; set; }

        [JsonPropertyName("tags")]
        public List<TagDto>? Tags { get; set; }
    }

    private class ArtistCreditDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private class ReleaseDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    private class TagDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}