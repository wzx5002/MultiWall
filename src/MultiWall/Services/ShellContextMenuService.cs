using System;
using Microsoft.Win32;

namespace MultiWall.Services;

public static class ShellContextMenuService
{
    private const string RegPath = @"Software\Classes\DesktopBackground\Shell\MultiWall.NextWallpaper";

    public static void Register()
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            if (string.IsNullOrEmpty(exePath)) return;

            using var key = Registry.CurrentUser.CreateSubKey(RegPath);
            key.SetValue(null, LocalizationService.GetString("Label.DesktopNextWallpaper"));
            key.SetValue("Icon", exePath);
            key.SetValue("Position", "Top");

            using var cmdKey = key.CreateSubKey("command");
            cmdKey.SetValue(null, $"\"{exePath}\" --next");
        }
        catch { }
    }

    public static void Unregister()
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(RegPath, throwOnMissingSubKey: false);
        }
        catch { }
    }
}
