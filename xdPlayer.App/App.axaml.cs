using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Services;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Domain.Playback;
using xdPlayer.Infrastructure.Data;
using xdPlayer.Infrastructure.Repositories;
using xdPlayer.Infrastructure.Services;

namespace xdPlayer.App;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // automatically use migrations every launch
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        if (!db.UserProfiles.Any())
        {
            db.UserProfiles.Add(new Domain.Entities.UserProfile());
            db.SaveChanges();
        }

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
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        /* services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        */
        services.AddSingleton<PlaybackQueue>();
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
        services.AddSingleton<PlaybackManager>();


        // ViewModels
        /*
        services.AddTransient<MainViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<PlayerViewModel>();
        */

        // Windows
        services.AddSingleton<MainWindow>();
    }
}