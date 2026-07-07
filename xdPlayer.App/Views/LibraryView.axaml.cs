using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Linq;
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

        AddHandler(PointerPressedEvent, OnPreviewPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control source && source.FindAncestorOfType<TextBox>() == null && source is not TextBox)
        {
            Focus();
        }
    }

    private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control)
            control.Focus();
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

    private async void OnEditTrackClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        if (menuItem.DataContext is not Track track)
            return;

        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var editWindow = new EditTrackWindow(track);
        await editWindow.ShowDialog(topLevel);

        if (editWindow.Confirmed)
        {
            var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
            await libraryVm.RefreshTrackAsync(track.Id);
        }
    }

    private void OnListDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.DataContext is not Track track)
            return;

        if (DataContext is LibraryViewModel vm)
            vm.DeleteTrackCommand.Execute(track).Subscribe();
    }

    private void OnListCoverPlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;

        if (sender is not Border border || border.DataContext is not Track track) return;

        if (DataContext is LibraryViewModel vm)
            vm.PlayTrackCommand.Execute(track).Subscribe();
    }

    private void OnListCoverPlayPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "ListCoverPlayOverlay") is Border overlay)
            overlay.Background = new SolidColorBrush(Color.Parse("#99000000"));
        if (border.GetVisualDescendants().OfType<Polygon>().FirstOrDefault(p => p.Name == "ListPlayIcon") is Polygon icon)
            icon.Opacity = 1;
    }

    private void OnListCoverPlayPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "ListCoverPlayOverlay") is Border overlay)
            overlay.Background = new SolidColorBrush(Color.Parse("#00000000"));
        if (border.GetVisualDescendants().OfType<Polygon>().FirstOrDefault(p => p.Name == "ListPlayIcon") is Polygon icon)
            icon.Opacity = 0;
    }
}