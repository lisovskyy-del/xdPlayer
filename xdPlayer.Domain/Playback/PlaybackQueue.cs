using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Playback;

public class PlaybackQueue
{
    private readonly List<Track> _tracks = new();

    public IReadOnlyList<Track> Tracks => _tracks;

    public int CurrentIndex { get; private set; }

    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;

    public PlaybackMode PlaybackMode { get; set; } = PlaybackMode.Normal;

    // checks what current track is. if its 0, it sends a signal to stop.
    public Track? CurrentTrack => _tracks.Count == 0 ? null : _tracks[CurrentIndex];


    public void Add(Track track) // adds a track to the queue
    {
        _tracks.Add(track);
    }

    public void AddRange(IEnumerable<Track> tracks) // adds a list of tracks to the queue
    {
        _tracks.AddRange(tracks);
    }

    public Track? Next() // plays next audio. goes through four checkers, if none passed, return null.
    {
        if (_tracks.Count == 0)
        {
            return null;
        }

        if (RepeatMode == RepeatMode.One) // if repeatmode is one, plays the current track again
        {
            return CurrentTrack;
        }

        if (CurrentIndex < _tracks.Count - 1) // simple check to see if we can play the next audio.
        {
            CurrentIndex++;
            return _tracks[CurrentIndex];
        }

        if (RepeatMode == RepeatMode.All) // if repeat mode is all, at the end of the playback it starts over.
        {
            CurrentIndex = 0;
            return _tracks[CurrentIndex];
        }

        return null;
    }

    public Track? Previous() // goes to the previous track
    {
        if (_tracks.Count == 0) // if there are no tracks, return null
        {
            return null;
        }

        if (CurrentIndex > 0) // if there are tracks, return the previous track
        {
            CurrentIndex--;
        }

        return _tracks[CurrentIndex];
    }

    public void Shuffle() // will shuffle tracks using an algorithm
    {
        Random rng = new();

        int n = _tracks.Count;

        while (n > 1)
        {
            n--;

            int k = rng.Next(n + 1);

            (_tracks[k], _tracks[n]) = (_tracks[n], _tracks[k]);
        }

        CurrentIndex = 0;
    }
}