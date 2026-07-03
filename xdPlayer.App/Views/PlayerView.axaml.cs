using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using System;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.App.Views;

public partial class PlayerView : UserControl
{
    public PlayerView()
    {
        InitializeComponent();

        var slider = this.FindControl<Slider>("ProgressSlider");
        if (slider != null)
        {
            slider.AddHandler(InputElement.PointerPressedEvent, OnSeekPointerPressed,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

            slider.AddHandler(InputElement.PointerReleasedEvent, OnSeekPointerReleased,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        }
    }

    private void OnSeekPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed)
            return;

        System.Diagnostics.Debug.WriteLine("[Seek] Pointer pressed");

        if (DataContext is PlayerViewModel vm)
            vm.IsSeeking = true;
    }

    private void OnSeekPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        if (sender is not Slider slider) return;
        if (DataContext is not PlayerViewModel vm) return;

        var seconds = slider.Value;
        System.Diagnostics.Debug.WriteLine($"[Seek] Slider released at {seconds}s");

        var playbackManager = App.Services.GetRequiredService<IPlaybackManager>();
        playbackManager.Seek(TimeSpan.FromSeconds(seconds));

        vm.CurrentPosition = TimeSpan.FromSeconds(seconds);
        vm.IsSeeking = false;
    }
}