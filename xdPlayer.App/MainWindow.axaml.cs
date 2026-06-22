using Avalonia.Controls;
using xdPlayer.Presentation.Design;
using xdPlayer.Presentation.ViewModels;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = DesignViewModelFactory.CreatePlayer();
        }
    }
}