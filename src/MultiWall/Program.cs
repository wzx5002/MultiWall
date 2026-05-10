using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;

namespace MultiWall;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.WriteAllText(logPath, ex.ToString());
            Environment.Exit(1);
        }
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name;
        var path = Path.Combine(AppContext.BaseDirectory, "Lib", name + ".dll");
        if (File.Exists(path))
        {
            try { return Assembly.LoadFrom(path); }
            catch { }
        }
        return null;
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
