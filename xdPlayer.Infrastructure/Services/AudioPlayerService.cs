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

        _audioFile = new AudioFileReader(path);
        _outputDevice = new WaveOutEvent();

        _outputDevice.Init(_audioFile); // pass the audio file to the device user is on

        _outputDevice.PlaybackStopped += OnPlaybackStopped;

        _outputDevice.Play(); // play the audio file
    }

    public void Pause()
    {
        _outputDevice?.Pause();
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
    }
}