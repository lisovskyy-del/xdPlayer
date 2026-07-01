using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using System.ComponentModel;
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
}