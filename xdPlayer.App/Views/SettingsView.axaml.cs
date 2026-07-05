using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        if (!Design.IsDesignMode)
            DataContext = App.Services?.GetRequiredService<SettingsViewModel>();

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new SettingsViewModel();
    }

    private void OnPresetColorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not string hex) return;
        if (DataContext is not SettingsViewModel vm) return;

        vm.SetPresetColorCommand.Execute(hex).Subscribe();
    }
}