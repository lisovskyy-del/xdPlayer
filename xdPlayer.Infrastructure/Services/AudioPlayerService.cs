using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.Infrastructure.Services;

public class AudioPlayerService : IAudioPlayerService
{
    private WaveOutEvent? _outputDevice;
    private AudioFileReader? _audioFile;
    private readonly SemaphoreSlim _playLock = new(1, 1);
    private float _volume = 1.0f;

    public bool IsPaused { get; private set; }

    public TimeSpan CurrentPosition => _audioFile?.CurrentTime ?? TimeSpan.Zero;
    public TimeSpan TotalDuration => _audioFile?.TotalTime ?? TimeSpan.Zero;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_outputDevice != null)
                _outputDevice.Volume = _volume;
        }
    }

    public event EventHandler? PlaybackStarted;
    public event EventHandler? PlaybackPaused;
    public event EventHandler? PlaybackFinished;

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        Debug.WriteLine($"[Audio] PlaybackStopped, Position={_audioFile?.Position}, Length={_audioFile?.Length}");

        if (_audioFile != null && _audioFile.Position >= _audioFile.Length - 10000)
        {
            Debug.WriteLine("[Audio] PlaybackFinished invoked");
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task PlayAsync(string path)
    {
        await _playLock.WaitAsync();
        try
        {
            Stop();
            IsPaused = false;

            await Task.Run(() =>
            {
                _audioFile = new AudioFileReader(path);
                _outputDevice = new WaveOutEvent();

                _outputDevice.Init(_audioFile);
                _outputDevice.Volume = _volume;
                _outputDevice.PlaybackStopped += OnPlaybackStopped;

                _outputDevice.Play();
            });

            PlaybackStarted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _playLock.Release();
        }
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

    public void Seek(TimeSpan position)
    {
        if (_audioFile == null) return;

        var clamped = position < TimeSpan.Zero ? TimeSpan.Zero
            : position > _audioFile.TotalTime ? _audioFile.TotalTime
            : position;

        _audioFile.CurrentTime = clamped;
    }

    public void Stop()
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