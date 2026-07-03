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
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class TrackCardView : UserControl
{
    public TrackCardView()
    {
        InitializeComponent();
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

    private async void OnChangeCoverClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not Track track) return;

        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose cover image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Images")
            {
                Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"]
            }
            ]
        });

        if (files.Count == 0) return;

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        await libraryVm.SetTrackCoverCommand.Execute((track, files[0].Path.LocalPath));
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

    private void OnCoverPlayPointerEntered(object? sender, PointerEventArgs e)
    {
        CoverPlayOverlay.Background = new SolidColorBrush(Color.Parse("#99000000"));
        PlayButtonCircle.Opacity = 1;
    }

    private void OnCoverPlayPointerExited(object? sender, PointerEventArgs e)
    {
        CoverPlayOverlay.Background = new SolidColorBrush(Color.Parse("#00000000"));
        PlayButtonCircle.Opacity = 0;
    }

    private void OnCoverPlayDoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }
}