using Avalonia;
using ReactiveUI.Avalonia;
using Serilog;
using System;
using System.Threading;
using xdPlayer.Application.Models;

namespace xdPlayer.App;

class Program
{
    private static Mutex? _mutex;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        const string mutexName = "xdPlayer_SingleInstance_Mutex";

        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            return;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/xdPlayer-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();

        try
        {
            Log.Information("Launching xdPlayer...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "App failed to launch");
        }
        finally
        {
            _mutex.ReleaseMutex();
            Log.CloseAndFlush();
        }
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .UseReactiveUI(_ => { })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
