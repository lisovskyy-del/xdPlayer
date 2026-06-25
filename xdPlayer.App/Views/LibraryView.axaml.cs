using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        if (!Design.IsDesignMode)
            DataContext = App.Services?.GetRequiredService<LibraryViewModel>();

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new LibraryViewModel();
    }

    private void OnTrackDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is Track track)
        {
            if (DataContext is LibraryViewModel vm)
                vm.PlayTrackCommand.Execute(track).Subscribe();
        }
    }
}