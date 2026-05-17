using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using MultiWall.Models;
using MultiWall.Services;

namespace MultiWall;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--next"))
        {
            RunNextWallpaper();
            return;
        }

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

    private static void RunNextWallpaper()
    {
        try
        {
            var config = ConfigService.Load();
            var wallpaperService = new WallpaperService();

            foreach (var mc in config.Monitors)
            {
                if (mc.Mode != WallpaperMode.Slideshow || !mc.IsSlideshowRunning || mc.SlideshowImages.Count == 0)
                    continue;

                // find current index by matching WallpaperPath
                int curIdx = 0;
                for (var i = 0; i < mc.SlideshowImages.Count; i++)
                {
                    if (string.Equals(mc.SlideshowImages[i], mc.WallpaperPath, StringComparison.OrdinalIgnoreCase))
                    {
                        curIdx = i;
                        break;
                    }
                }

                int nextIdx;
                if (mc.SlideshowShuffle && mc.SlideshowImages.Count > 1)
                    nextIdx = new Random().Next(mc.SlideshowImages.Count);
                else
                    nextIdx = (curIdx + 1) % mc.SlideshowImages.Count;

                var nextPath = mc.SlideshowImages[nextIdx];
                wallpaperService.SetWallpaper(mc.DevicePath, nextPath);
                mc.WallpaperPath = nextPath;
            }

            ConfigService.Save(config);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "next.log"), ex.ToString());
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
