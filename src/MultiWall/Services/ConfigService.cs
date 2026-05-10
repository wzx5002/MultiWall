using System;
using System.IO;
using System.Text.Json;
using MultiWall.Models;

namespace MultiWall.Services;

public static class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "config.json");

    private static string ErrorLogPath => Path.Combine(AppContext.BaseDirectory, "error.log");

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                if (config != null) return config;
            }
        }
        catch (Exception ex)
        {
            try { File.WriteAllText(ErrorLogPath, $"ConfigService.Load failed: {ex}"); } catch { }
        }
        return new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            try { File.WriteAllText(ErrorLogPath, $"ConfigService.Save failed: {ex}"); } catch { }
        }
    }
}
