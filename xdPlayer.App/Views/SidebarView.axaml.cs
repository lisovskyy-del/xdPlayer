using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
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