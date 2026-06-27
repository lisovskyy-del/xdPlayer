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

    public MainWindow(PlayerViewModel playerVm)
    {
        InitializeComponent();
        DataContext = playerVm;

        PointerPressed += (_, e) =>
        {
            var hit = e.Source as Control;
            while (hit != null && hit is not ListBoxItem)
                hit = hit.Parent as Control;

            if (hit == null)
            {
                foreach (var listBox in this.GetVisualDescendants().OfType<ListBox>())
                    listBox.SelectedItem = null;
            }
        };
    }
}