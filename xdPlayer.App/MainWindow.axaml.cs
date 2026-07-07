using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    private bool _isShuttingDown = false;

    public MainWindow(ViewModels.MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Focusable = true;

        Closing += MainWindow_Closing;

        AddHandler(PointerPressedEvent, OnGlobalPointerPressed, RoutingStrategies.Tunnel);
    }

    public MainWindow() : this(null!)
    {
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_isShuttingDown)
        {
            return;
        }

        e.Cancel = true;

        Hide();
        ShowInTaskbar = false;
    }

    public void ForceShutdown()
    {
        _isShuttingDown = true;
        Close();
    }

    private void OnGlobalPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control source) return;

        if (source is TextBox || source.FindAncestorOfType<TextBox>() != null)
            return;

        Focus();
    }
}
