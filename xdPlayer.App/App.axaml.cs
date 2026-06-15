using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using System;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.App;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DB - SQLite
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=xdPlayer.db"));

        // Repositories
        /* services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();

        // Services
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<PlayerViewModel>();
        */

        // Windows
        services.AddSingleton<MainWindow>();
    }
}