using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows.Input;
using xdPlayer.App.ViewModels;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Models;
using xdPlayer.Application.Services;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Domain.Playback;
using xdPlayer.Infrastructure.Data;
using xdPlayer.Infrastructure.Repositories;
using xdPlayer.Infrastructure.Services;
using Avalonia.Threading;

namespace xdPlayer.App;

internal class ActionCommand : ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action) => _action = action;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _action();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

            var mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;

            var iconUri = new Uri("avares://xdPlayer.App/Assets/xdPlayer.ico");

            var restoreWindowAction = new Action(() =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.ShowInTaskbar = true;
                    mainWindow.Activate();
                });
            });

            var trayIcons = new TrayIcons
            {
                new TrayIcon
                {
                    Icon = new WindowIcon(new Bitmap(AssetLoader.Open(iconUri))),
                    ToolTipText = "xdPlayer",

                    Menu = new NativeMenu
                    {
                        Items =
                        {
                            new NativeMenuItem("Exit")
                            {
                                Command = new ActionCommand(() =>
                                {
                                    Dispatcher.UIThread.Post(() =>
                                    {
                                        mainWindow.ForceShutdown();
                                        desktop.Shutdown();
                                    });
                                })
                            }
                        }
                    }
                }
            };

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

            var playlistVm = Services.GetRequiredService<PlaylistViewModel>();
            await playlistVm.RefreshPlaylistsAsync();

            var statsService = Services.GetRequiredService<IStatisticsService>();
            await statsService.BackfillDailyStatisticsAsync();

            var themeService = Services.GetRequiredService<IThemeService>();
            var settings = await themeService.LoadSettingsAsync();
            themeService.ApplyAccentColor(settings.AccentColor);


            Avalonia.Application.Current!.SetValue(TrayIcon.IconsProperty, trayIcons);
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
        services.AddScoped<IListeningSessionRepository, ListeningSessionRepository>();
        services.AddScoped<IDailyStatisticsRepository, DailyStatisticsRepository>();

        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ITagService, TagService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();

        services.AddSingleton<ListeningSessionService>();
        services.AddSingleton<PlaybackQueue>();
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();
        services.AddSingleton<IPlaybackManager, PlaybackManager>();
        services.AddSingleton<IMetadataReader, TagLibMetadataReader>();
        services.AddSingleton<IMetadataEnrichmentService, MetadataEnrichmentService>();

        services.AddHttpClient<IMusicBrainzClient, MusicBrainzClient>();

        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<LibraryViewModel>();
        services.AddSingleton<PlaylistViewModel>();
        services.AddSingleton<SidebarViewModel>();
        services.AddSingleton<PlayerViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ProfileViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }
}