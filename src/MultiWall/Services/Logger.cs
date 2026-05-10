using System;
using System.IO;

namespace MultiWall.Services;

public static class Logger
{
    private static string? _path;
    public static bool Enabled { get; private set; }

    public static void Init(bool enable, string? baseDir = null)
    {
        Enabled = enable;
        if (enable)
        {
            _path = Path.Combine(baseDir ?? AppContext.BaseDirectory, "debug.log");
            Info("Logger", "Debug logging enabled");
        }
    }

    public static void Info(string source, string msg)
    {
        if (!Enabled || _path == null) return;
        Write(source, "INFO", msg);
    }

    public static void Error(string source, string msg)
    {
        Write(source, "ERROR", msg);
    }

    public static void Error(string source, Exception ex)
    {
        Write(source, "ERROR", $"{ex.GetType().Name}: {ex.Message}\n{ex}");
    }

    private static void Write(string source, string level, string msg)
    {
        try
        {
            File.AppendAllText(_path!, $"{DateTime.Now:HH:mm:ss.fff} [{level}] [{source}] {msg}{Environment.NewLine}");
        }
        catch { }
    }
}
