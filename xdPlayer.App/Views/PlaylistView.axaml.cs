using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    public PlaylistView()
    {
        if (!Design.IsDesignMode)
        {
            var vm = App.Services?.GetRequiredService<PlaylistViewModel>();
            System.Diagnostics.Debug.WriteLine($"[PlaylistView] VM HashCode: {vm?.GetHashCode()}");
            DataContext = vm;
        }

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new PlaylistViewModel();

        PointerReleased += OnTrackPointerReleased;
    }

    private void OnTrackDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            if (DataContext is PlaylistViewModel vm)
                vm.PlayTrackCommand.Execute(track).Subscribe();
        }
    }

    private void OnTrackPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            _draggedTrack = track;
            _isDragging = true;
        }
    }

    private void OnTrackPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging || _draggedTrack == null)
        {
            _isDragging = false;
            _draggedTrack = null;
            return;
        }

        var position = e.GetPosition(this);
        var targetElement = this.InputHitTest(position);

        var element = targetElement as Control;
        while (element != null && !(element is Grid grid && grid.DataContext is Track))
            element = element.Parent as Control;

        if (element is Grid targetGrid && targetGrid.DataContext is Track targetTrack &&
            DataContext is PlaylistViewModel vm)
        {
            var fromIndex = vm.CurrentTracks.IndexOf(_draggedTrack);
            var toIndex = vm.CurrentTracks.IndexOf(targetTrack);

            System.Diagnostics.Debug.WriteLine($"[DnD] from={_draggedTrack.Title}({fromIndex}) to={targetTrack.Title}({toIndex})");

            if (fromIndex >= 0 && toIndex >= 0 && fromIndex != toIndex)
            {
                System.Diagnostics.Debug.WriteLine($"[DnD] Executing MoveTrackCommand from={fromIndex} to={toIndex}");
                vm.MoveTrackCommand.Execute((fromIndex, toIndex)).Subscribe(
                    _ => System.Diagnostics.Debug.WriteLine("[DnD] Command executed"),
                    ex => System.Diagnostics.Debug.WriteLine($"[DnD] Command error: {ex.Message}")
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DnD] Skipped: fromIndex={fromIndex} toIndex={toIndex}");
            }
        }

        _draggedTrack = null;
        _isDragging = false;
    }

    private void OnTrackDrop(object? sender, DragEventArgs e)
    {
        if (_draggedTrack == null) return;
        if (sender is not Grid grid) return;
        if (grid.DataContext is not Track targetTrack) return;
        if (DataContext is not PlaylistViewModel vm) return;

        var fromIndex = vm.CurrentTracks.IndexOf(_draggedTrack);
        var toIndex = vm.CurrentTracks.IndexOf(targetTrack);

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;

        System.Diagnostics.Debug.WriteLine($"[DnD] Executing MoveTrackCommand from={fromIndex} to={toIndex}");
        vm.MoveTrackCommand.Execute((fromIndex, toIndex)).Subscribe(
            _ => System.Diagnostics.Debug.WriteLine("[DnD] Command executed"),
            ex => System.Diagnostics.Debug.WriteLine($"[DnD] Command error: {ex.Message}")
        );
    }

    private async void OnDeletePlaylistClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Playlist playlist)
        {
            System.Diagnostics.Debug.WriteLine($"[View] DataContext VM HashCode: {(DataContext as PlaylistViewModel)?.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"[View] Delete clicked for playlist: {playlist.Name}");
            if (DataContext is PlaylistViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine($"[View] Executing DeletePlaylistCommand");
                vm.DeletePlaylistCommand.Execute(playlist).Subscribe(
                    _ => System.Diagnostics.Debug.WriteLine("[View] Command completed"),
                    ex => System.Diagnostics.Debug.WriteLine($"[View] Command error: {ex.Message}")
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[View] DataContext is {DataContext?.GetType().Name}");
            }
        }
    }

    private async void OnRemoveTrackClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Track track)
        {
            if (DataContext is PlaylistViewModel vm)
                await vm.RemoveTrackCommand.Execute(track);
        }
    }
}