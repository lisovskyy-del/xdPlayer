using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.Infrastructure.Services;

public class AudioPlayerService : IAudioPlayerService
{
    private WaveOutEvent? _outputDevice; // device of the user
    private AudioFileReader? _audioFile; // opens and plays the audio file

    public bool IsPaused { get; private set; }

    public event EventHandler? PlaybackStarted;
    public event EventHandler? PlaybackPaused;
    public event EventHandler? PlaybackFinished;

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (_audioFile != null && _audioFile.Position >= _audioFile.Length)
        {
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Play(string path)
    {
        Stop();
        IsPaused = false;

        _audioFile = new AudioFileReader(path);
        _outputDevice = new WaveOutEvent();

        _outputDevice.Init(_audioFile); // pass the audio file to the device user is on

        _outputDevice.PlaybackStopped += OnPlaybackStopped;

        _outputDevice.Play(); // play the audio file
        PlaybackStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        if (_outputDevice == null) return;

        _outputDevice?.Pause();
        IsPaused = true;
        PlaybackPaused?.Invoke(this, EventArgs.Empty);
    }

    public void Resume()
    {
        _outputDevice?.Play();
        IsPaused = false;
        PlaybackStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Stop() // clears the audiofile that was about to be passed to the device and the device itself
    {
        if (_outputDevice != null)
        {
            _outputDevice.PlaybackStopped -= OnPlaybackStopped;

            _outputDevice?.Stop();

            _outputDevice?.Dispose();

            _outputDevice = null;
        }

        _audioFile?.Dispose();
        _audioFile = null;

        IsPaused = false;
    }
}