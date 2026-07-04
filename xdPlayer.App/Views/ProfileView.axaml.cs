using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        if (!Design.IsDesignMode)
            DataContext = App.Services?.GetRequiredService<ProfileViewModel>();

        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new ProfileViewModel();
    }
}