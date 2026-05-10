using System;
using System.Linq;
using Avalonia;
using MultiWall.Services;

namespace MultiWall;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var debug = args.Any(a => a.Equals("--debug", StringComparison.OrdinalIgnoreCase)
                               || a.Equals("-d", StringComparison.OrdinalIgnoreCase));
        Logger.Init(debug);

        try
        {
            Logger.Info("Program", "Starting");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Logger.Error("Program", ex);
            Environment.Exit(1);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
