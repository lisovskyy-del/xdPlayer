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
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new LibraryViewModel();
            return;
        }

        Loaded += (_, _) =>
        {
            DataContext = App.Services.GetRequiredService<LibraryViewModel>();
        };
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