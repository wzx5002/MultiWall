using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace MultiWall.Services;

[SupportedOSPlatform("windows")]
public static class AutoStartService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "MultiWall";

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey);
                return key?.GetValue(AppName) != null;
            }
            catch { return false; }
        }
    }

    public static void SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (key == null) return;

            if (enable)
                key.SetValue(AppName, Environment.ProcessPath ?? "");
            else
                key.DeleteValue(AppName, false);
        }
        catch { }
    }
}
