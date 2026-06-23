using Avalonia.Controls;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    public MainWindow(PlayerViewModel vm, IPlaybackManager playbackManager, PlaybackQueue queue)
    {
        InitializeComponent();
        DataContext = vm;

        queue.Add(new Track
        {
            Title = "DJ AGONY - BASS CUTS [FULL TAPE]",
            FilePath = @"C:\Users\manin\Downloads\imp\audios\DJ AGONY - BASS CUTS [FULL TAPE].mp3"
        });
    }
}