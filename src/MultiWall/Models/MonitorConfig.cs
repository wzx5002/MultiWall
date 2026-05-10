using System.Collections.Generic;
using System.Text.Json.Serialization;
using MultiWall.Services;

namespace MultiWall.Models;

public class MonitorConfig
{
    public string DevicePath { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WallpaperMode Mode { get; set; } = WallpaperMode.SingleImage;

    public string WallpaperPath { get; set; } = "";
    public List<string> SlideshowImages { get; set; } = [];
    public int SlideshowInterval { get; set; } = 60;
    public bool IsSlideshowRunning { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DesktopWallpaperPosition Position { get; set; } = DesktopWallpaperPosition.Fill;
}
