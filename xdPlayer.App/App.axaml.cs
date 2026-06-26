using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Domain.Playback;
using xdPlayer.Infrastructure.Data;
using xdPlayer.Infrastructure.Repositories;
using xdPlayer.Infrastructure.Services;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Models;
using xdPlayer.Application.Services;

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
            desktop.Exit += async (_, _) =>
            {
                var sessionService = Services.GetRequiredService<ListeningSessionService>();
                await sessionService.OnTrackEndedAsync(completed: false);
            };

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

        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        // services.AddScoped<IStatisticsService, StatisticsService>();

        services.AddSingleton<ListeningSessionService>();
        services.AddScoped<IListeningSessionRepository, ListeningSessionRepository>();
        services.AddSingleton<PlaybackQueue>();
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
        services.AddSingleton<IPlaybackManager, PlaybackManager>();
        services.AddSingleton<IMetadataReader, TagLibMetadataReader>();


        // services.AddTransient<MainViewModel>();
        services.AddSingleton<LibraryViewModel>();
        services.AddSingleton<PlaylistViewModel>();
        services.AddTransient<PlayerViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }
}