using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class LibraryView : UserControl
{
    private Track? _contextMenuTrack;

    public LibraryView()
    {
        if (!Design.IsDesignMode)
            DataContext = App.Services?.GetRequiredService<LibraryViewModel>();

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new LibraryViewModel();
    }

    private void OnGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
            _contextMenuTrack = track;
    }

    private void OnTrackDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            if (DataContext is LibraryViewModel vm)
                vm.PlayTrackCommand.Execute(track).Subscribe();
        }
    }

    private async void OnAddToPlaylist(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        if (menuItem.DataContext is not Playlist playlist) return;
        if (_contextMenuTrack == null) return;

        System.Diagnostics.Debug.WriteLine($"[Library] Adding track {_contextMenuTrack.Title} to playlist {playlist.Name}");

        if (DataContext is LibraryViewModel vm)
        {
            await vm.AddToPlaylistCommand.Execute((_contextMenuTrack, playlist));
            var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();
            await playlistVm.RefreshAsync();
        }
    }

    // --- List view mode ---

    private void OnListTrackDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            if (DataContext is LibraryViewModel vm)
                vm.PlayTrackCommand.Execute(track).Subscribe();
        }
    }

    private void OnListContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (sender is not ContextMenu contextMenu) return;
        if (_contextMenuTrack == null) return;
        if (contextMenu.Items.Count == 0) return;
        if (contextMenu.Items[0] is not MenuItem addToPlaylistItem) return;

        var track = _contextMenuTrack;
        var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();
        addToPlaylistItem.Items.Clear();

        if (playlistVm.Playlists.Count == 0)
        {
            addToPlaylistItem.Items.Add(new MenuItem
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
                if (DataContext is LibraryViewModel vm)
                    vm.AddToPlaylistCommand.Execute((track, playlist)).Subscribe();
            };

            addToPlaylistItem.Items.Add(item);
        }
    }

    private void OnListDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.DataContext is not Track track)
            return;

        if (DataContext is LibraryViewModel vm)
            vm.DeleteTrackCommand.Execute(track).Subscribe();
    }
}