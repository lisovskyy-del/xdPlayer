using Avalonia;
using Avalonia.Controls;
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
using xdPlayer.App.ViewModels;

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
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);

        Services = services.BuildServiceProvider();

        // do migrations every launch
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            using var scope = Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            if (!db.UserProfiles.Any())
            {
                db.UserProfiles.Add(new Domain.Entities.UserProfile());
                db.SaveChanges();
            }

            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // DB - SQLite
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=xdPlayer.db"));

        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        /* services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        */

        services.AddSingleton<ListeningSessionService>();
        services.AddScoped<IListeningSessionRepository, ListeningSessionRepository>();
        services.AddSingleton<PlaybackQueue>();
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
        services.AddSingleton<IPlaybackManager, PlaybackManager>();
        services.AddSingleton<IMetadataReader, TagLibMetadataReader>();


        /*
        services.AddTransient<MainViewModel>();
        services.AddTransient<LibraryViewModel>();
        */

        services.AddTransient<PlayerViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }
}