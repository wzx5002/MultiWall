using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Avalonia;

namespace MultiWall;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var libPath = Path.Combine(AppContext.BaseDirectory, "lib");
        if (Directory.Exists(libPath))
        {
            AssemblyLoadContext.Default.Resolving += (ctx, name) =>
            {
                var path = Path.Combine(libPath, name.Name + ".dll");
                if (File.Exists(path))
                    return ctx.LoadFromAssemblyPath(path);
                return null;
            };
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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
