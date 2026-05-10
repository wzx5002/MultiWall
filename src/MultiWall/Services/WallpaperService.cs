using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MultiWall.Models;

namespace MultiWall.Services;

public class WallpaperService : IWallpaperService, IDisposable
{
    private readonly IDesktopWallpaper? _wallpaper;

    public bool IsAvailable => _wallpaper != null;

    public WallpaperService()
    {
        if (!OperatingSystem.IsWindows())
        {
            _wallpaper = null;
            return;
        }

        try
        {
            var type = Type.GetTypeFromCLSID(typeof(DesktopWallpaperClass).GUID);
            if (type != null)
                _wallpaper = (IDesktopWallpaper)Activator.CreateInstance(type)!;
        }
        catch
        {
            _wallpaper = null;
        }
    }

    public List<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();

        if (_wallpaper == null)
            return monitors;

        try
        {
            _wallpaper.GetMonitorDevicePathCount(out uint count);

            for (uint i = 0; i < count; i++)
            {
                try
                {
                    _wallpaper.GetMonitorDevicePathAt(i, out string devicePath);
                    _wallpaper.GetMonitorRECT(devicePath, out RECT rect);
                    _wallpaper.GetWallpaper(devicePath, out string wallpaper);

                    monitors.Add(new MonitorInfo
                    {
                        Index = (int)i,
                        DevicePath = devicePath,
                        Left = rect.Left,
                        Top = rect.Top,
                        Right = rect.Right,
                        Bottom = rect.Bottom,
                        WallpaperPath = wallpaper ?? string.Empty
                    });
                }
                catch (COMException)
                {
                }
            }
        }
        catch (COMException)
        {
        }

        return monitors;
    }

    public void SetWallpaper(string monitorId, string imagePath)
    {
        if (_wallpaper == null)
            throw new PlatformNotSupportedException("Wallpaper service is only available on Windows.");
        _wallpaper.SetWallpaper(monitorId, imagePath);
    }

    public string GetWallpaper(string monitorId)
    {
        if (_wallpaper == null)
            return string.Empty;

        try
        {
            _wallpaper.GetWallpaper(monitorId, out string wallpaper);
            return wallpaper ?? string.Empty;
        }
        catch (COMException)
        {
            return string.Empty;
        }
    }

    public void ClearWallpaper(string monitorId)
    {
        if (_wallpaper == null)
            throw new PlatformNotSupportedException("Wallpaper service is only available on Windows.");
        _wallpaper.SetWallpaper(monitorId, string.Empty);
    }

    public void SetPosition(DesktopWallpaperPosition position)
    {
        _wallpaper?.SetPosition(position);
    }

    public DesktopWallpaperPosition GetPosition()
    {
        if (_wallpaper == null)
            return DesktopWallpaperPosition.Fill;

        try
        {
            _wallpaper.GetPosition(out DesktopWallpaperPosition position);
            return position;
        }
        catch (COMException)
        {
            return DesktopWallpaperPosition.Fill;
        }
    }

    public void Dispose()
    {
        if (_wallpaper != null && OperatingSystem.IsWindows() && Marshal.IsComObject(_wallpaper))
            Marshal.ReleaseComObject(_wallpaper);
    }
}
