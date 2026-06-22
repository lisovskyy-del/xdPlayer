using Avalonia.Controls;
using xdPlayer.Presentation.Design;

namespace xdPlayer.Presentation.Views;

public partial class PlayerView : UserControl
{
    public PlayerView()
    {
        InitializeComponent();

        if (Avalonia.Controls.Design.IsDesignMode)
        {
            DataContext = DesignViewModelFactory.CreatePlayer();
        }
    }
}