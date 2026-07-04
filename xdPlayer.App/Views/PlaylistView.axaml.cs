using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reactive.Linq;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class PlaylistView : UserControl
{
    private Track? _draggedTrack;
    private bool _isDragging;
    private Point _dragStartPosition;

    public PlaylistView()
    {
        if (!Design.IsDesignMode)
        {
            var vm = App.Services?.GetRequiredService<PlaylistViewModel>();
            DataContext = vm;
        }

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new PlaylistViewModel();
    }

    private void OnPlaylistCoverPointerEntered(object? sender, PointerEventArgs e)
    {
        PlaylistCoverOverlay.Background = new SolidColorBrush(Color.Parse("#99000000"));
        PlaylistPlayButtonCircle.Opacity = 1;
    }

    private void OnPlaylistCoverPointerExited(object? sender, PointerEventArgs e)
    {
        PlaylistCoverOverlay.Background = new SolidColorBrush(Color.Parse("#00000000"));
        PlaylistPlayButtonCircle.Opacity = 0;
    }

    private void OnTrackPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            _draggedTrack = track;
            _isDragging = true;
            _dragStartPosition = e.GetPosition(this);
        }
    }

    private void OnTrackGridPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _draggedTrack == null)
        {
            _isDragging = false;
            _draggedTrack = null;
            return;
        }

        var releasePosition = e.GetPosition(this);
        var distance = Math.Sqrt(
            Math.Pow(releasePosition.X - _dragStartPosition.X, 2) +
            Math.Pow(releasePosition.Y - _dragStartPosition.Y, 2));

        // Якщо курсор майже не рухався — це клік, не drag&drop
        if (distance < 4)
        {
            if (sender is Grid clickedGrid && clickedGrid.DataContext is Track clickedTrack &&
                DataContext is PlaylistViewModel playVm)
            {
                playVm.PlayTrackCommand.Execute(clickedTrack).Subscribe();
            }

            _draggedTrack = null;
            _isDragging = false;
            return;
        }

        var targetElement = this.InputHitTest(releasePosition);
        var element = targetElement as Control;
        while (element != null && !(element is Grid g && g.DataContext is Track))
            element = element.Parent as Control;

        if (element is Grid targetGrid && targetGrid.DataContext is Track targetTrack &&
            DataContext is PlaylistViewModel vm)
        {
            var fromIndex = vm.CurrentTracks.IndexOf(_draggedTrack);
            var toIndex = vm.CurrentTracks.IndexOf(targetTrack);

            if (fromIndex >= 0 && toIndex >= 0 && fromIndex != toIndex)
            {
                vm.MoveTrackCommand.Execute((fromIndex, toIndex)).Subscribe();
            }
        }

        _draggedTrack = null;
        _isDragging = false;
    }

    private async void OnDeletePlaylistClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Playlist playlist)
        {
            if (DataContext is PlaylistViewModel vm)
                vm.DeletePlaylistCommand.Execute(playlist).Subscribe();
        }
    }

    private void OnDeletePlaylistFromHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PlaylistViewModel vm || vm.SelectedPlaylist == null) return;
        vm.DeletePlaylistCommand.Execute(vm.SelectedPlaylist).Subscribe();
    }

    private async void OnRemoveTrackClick(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is Track track)
        {
            if (DataContext is PlaylistViewModel vm)
                await vm.RemoveTrackCommand.Execute(track);
        }
    }

    private void OnPlaylistCoverPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            return;

        if (DataContext is not PlaylistViewModel vm || vm.SelectedPlaylist == null) return;

        vm.PlayPlaylistCommand.Execute(vm.SelectedPlaylist).Subscribe();
    }

    private async void OnChangeCoverClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PlaylistViewModel vm || vm.SelectedPlaylist == null) return;

        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose playlist cover",
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

        await vm.SetPlaylistCoverCommand.Execute((vm.SelectedPlaylist, files[0].Path.LocalPath));
    }
}