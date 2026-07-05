using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
    }

    private void OnSidebarPlaylistCoverPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;

        if (sender is not Border border || border.DataContext is not Playlist playlist) return;

        var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();
        playlistVm.PlayPlaylistCommand.Execute(playlist).Subscribe();
    }

    private void OnSidebarPlaylistCoverPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "SidebarPlaylistCoverOverlay") is Border overlay)
            overlay.Background = new SolidColorBrush(Color.Parse("#99000000"));
        if (border.GetVisualDescendants().OfType<Polygon>().FirstOrDefault(p => p.Name == "SidebarPlaylistPlayIcon") is Polygon icon)
            icon.Opacity = 1;
    }

    private void OnSidebarPlaylistCoverPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "SidebarPlaylistCoverOverlay") is Border overlay)
            overlay.Background = new SolidColorBrush(Color.Parse("#00000000"));
        if (border.GetVisualDescendants().OfType<Polygon>().FirstOrDefault(p => p.Name == "SidebarPlaylistPlayIcon") is Polygon icon)
            icon.Opacity = 0;
    }

    private void OnSettingsRowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SidebarViewModel vm)
            vm.ShowSettingsCommand.Execute().Subscribe();
    }

    private async void OnChangePlaylistCoverClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.DataContext is not Playlist playlist) return;

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

        var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();

        await playlistVm.SetPlaylistCoverCommand
            .Execute((playlist, files[0].Path.LocalPath))
            .ToTask();
    }

    private void OnDeletePlaylistClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.DataContext is not Playlist playlist)
            return;

        var playlistVm = App.Services.GetRequiredService<PlaylistViewModel>();
        playlistVm.DeletePlaylistCommand.Execute(playlist).Subscribe();
    }

    private void OnAddFileClick(object? sender, RoutedEventArgs e)
    {
        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        libraryVm.AddFileCommand.Execute().Subscribe();
    }

    private void OnAddFolderClick(object? sender, RoutedEventArgs e)
    {
        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        libraryVm.AddFolderCommand.Execute().Subscribe();
    }

    private void OnAllTracksPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control control && control.FindAncestorOfType<Button>() != null)
            return;

        if (DataContext is SidebarViewModel vm)
            vm.ShowLibraryCommand.Execute().Subscribe();
    }

    private void OnUserFooterPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is SidebarViewModel vm)
            vm.ShowProfileCommand.Execute().Subscribe();
    }
}