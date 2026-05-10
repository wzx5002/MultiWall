using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;

namespace MultiWall;

sealed class Program
{
    private static string? _logPath;

    [STAThread]
    public static void Main(string[] args)
    {
        _logPath = Path.Combine(AppContext.BaseDirectory, "debug.log");
        Log("Program.Main started");

        try
        {
            Log("Building Avalonia app...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            Log("StartWithClassicDesktopLifetime returned normally");
        }
        catch (Exception ex)
        {
            Log("EXCEPTION: " + ex);
            var crashPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.WriteAllText(crashPath, ex.ToString());
            Environment.Exit(1);
        }
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name;
        var path = Path.Combine(AppContext.BaseDirectory, "Lib", name + ".dll");
        Log($"AssemblyResolve: {args.Name} -> {path} (exists={File.Exists(path)})");
        if (File.Exists(path))
        {
            try
            {
                var asm = Assembly.LoadFrom(path);
                Log($"  loaded OK: {asm.FullName}");
                return asm;
            }
            catch (Exception ex)
            {
                Log($"  LoadFrom failed: {ex.Message}");
            }
        }
        Log($"  not found, returning null");
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

    internal static void Log(string msg)
    {
        try
        {
            File.AppendAllText(_logPath ?? "debug.log", $"{DateTime.Now:HH:mm:ss.fff} [Program] {msg}{Environment.NewLine}");
        }
        catch { }
    }
}

internal static class ModuleInitializer
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = new AssemblyName(args.Name).Name;
            var path = Path.Combine(AppContext.BaseDirectory, "Lib", name + ".dll");
            if (File.Exists(path))
            {
                try { return Assembly.LoadFrom(path); }
                catch { }
            }
            return null;
        };
    }
}
