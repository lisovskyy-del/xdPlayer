using Avalonia.Controls;
using Avalonia.Interactivity;
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
}