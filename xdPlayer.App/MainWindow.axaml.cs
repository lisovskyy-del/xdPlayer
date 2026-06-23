using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using xdPlayer.App.ViewModels;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App;

public partial class MainWindow : Window
{
    public MainWindow(PlayerViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}