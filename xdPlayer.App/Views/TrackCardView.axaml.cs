using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class TrackCardView : UserControl
{
    private PlayerViewModel? _playerVm;

    public TrackCardView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) => UpdatePlayingState();

        _playerVm = App.Services?.GetRequiredService<PlayerViewModel>();
        if (_playerVm != null)
            _playerVm.CurrentTrackChanged += OnCurrentTrackChanged;

        Unloaded += (_, _) =>
        {
            if (_playerVm != null)
                _playerVm.CurrentTrackChanged -= OnCurrentTrackChanged;
        };

        UpdatePlayingState();
    }

    private void OnCurrentTrackChanged(object? sender, int trackId)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdatePlayingState);
    }

    private void UpdatePlayingState()
    {
        if (DataContext is not Track track || _playerVm == null)
        {
            Card.BorderBrush = Brushes.Transparent;
            return;
        }

        Card.BorderBrush = track.Id == _playerVm.CurrentTrackId
            ? (IBrush)Avalonia.Application.Current!.Resources["AccentBrush"]
            : Brushes.Transparent;
    }

    private async void OnEditTrackClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not Track track) return;

        var libraryService = App.Services.GetRequiredService<ILibraryService>();
        var fullTrack = await libraryService.GetByIdWithTagsAsync(track.Id);
        if (fullTrack == null) return;

        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var editWindow = new EditTrackWindow(fullTrack);
        await editWindow.ShowDialog(topLevel);

        if (editWindow.Confirmed)
        {
            var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
            await libraryVm.RefreshTrackAsync(track.Id);
        }
    }

    private void OnContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (DataContext is not Track track) return;

        var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();

        AddToPlaylistMenuItem.Items.Clear();

        if (playlistVm.Playlists.Count == 0)
        {
            AddToPlaylistMenuItem.Items.Add(new MenuItem
            {
                Header = "No playlists yet",
                IsEnabled = false
            });
            return;
        }

        foreach (var playlist in playlistVm.Playlists)
        {
            var item = new MenuItem { Header = playlist.Name };

            item.Click += (_, _) =>
            {
                playlistVm.AddTrackToPlaylistCommand.Execute((track, playlist))
                    .Subscribe();
            };

            AddToPlaylistMenuItem.Items.Add(item);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not Track track)
            return;

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();

        libraryVm.PlayTrackCommand.Execute(track).Subscribe();
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not Track track) return;

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        libraryVm.DeleteTrackCommand.Execute(track).Subscribe();
    }

    private void OnCoverPlayPointerEntered(object? sender, PointerEventArgs e)
    {
        CoverPlayOverlay.Background = new SolidColorBrush(Color.Parse("#77000000"));
        PlayButtonCircle.Opacity = 1;

        if (PlayButtonCircle.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 1;
            scale.ScaleY = 1;
        }
    }

    private void OnCoverPlayPointerExited(object? sender, PointerEventArgs e)
    {
        CoverPlayOverlay.Background = new SolidColorBrush(Color.Parse("#00000000"));
        PlayButtonCircle.Opacity = 0;

        if (PlayButtonCircle.RenderTransform is ScaleTransform scale)
        {
            scale.ScaleX = 0.9;
            scale.ScaleY = 0.9;
        }
    }

    private void OnCoverPlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;

        if (DataContext is not Track track) return;

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        libraryVm.PlayTrackCommand.Execute(track).Subscribe();
    }

    private void OnCoverPlayDoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }
}