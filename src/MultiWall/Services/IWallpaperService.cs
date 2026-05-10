using System.Collections.Generic;
using MultiWall.Models;

namespace MultiWall.Services;

public interface IWallpaperService
{
    bool IsAvailable { get; }
    List<MonitorInfo> GetMonitors();
    void SetWallpaper(string monitorId, string imagePath);
    string GetWallpaper(string monitorId);
    void ClearWallpaper(string monitorId);
    void SetPosition(DesktopWallpaperPosition position);
    DesktopWallpaperPosition GetPosition();
}
