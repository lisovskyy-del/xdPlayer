using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using xdPlayer.App.Views;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(PlayerViewModel playerVm)
    {
        InitializeComponent();
        DataContext = playerVm;
    }
}