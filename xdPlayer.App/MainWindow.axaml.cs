using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using xdPlayer.App.ViewModels;
using xdPlayer.App.Views;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Focusable = true;

        AddHandler(PointerPressedEvent, OnGlobalPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnGlobalPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control source) return;

        if (source is TextBox || source.FindAncestorOfType<TextBox>() != null)
            return;

        Focus();
    }
}