using Avalonia.Controls;
using Avalonia.Input;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Views;

public partial class PlayerView : UserControl
{
    public PlayerView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new PlayerViewModel();
    }
}