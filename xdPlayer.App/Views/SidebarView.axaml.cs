using Avalonia.Controls;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Views;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
        DataContext = new PlaylistViewModel();
    }
}