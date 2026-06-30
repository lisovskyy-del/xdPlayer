using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class TrackCardView : UserControl
{
    public TrackCardView()
    {
        InitializeComponent();
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