using System;
using System.Runtime.InteropServices;

namespace MultiWall.Services;

[ComImport]
[Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
internal class DesktopWallpaperClass { }

public enum DesktopWallpaperPosition
{
    Center = 0,
    Tile = 1,
    Stretch = 2,
    Fit = 3,
    Fill = 4,
    Span = 5
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDesktopWallpaper
{
    void SetWallpaper(
        [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
        [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

    void GetWallpaper(
        [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
        [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);

    void GetMonitorDevicePathAt(
        uint monitorIndex,
        [MarshalAs(UnmanagedType.LPWStr)] out string monitorID);

    void GetMonitorDevicePathCount(out uint count);

    void GetMonitorRECT(
        [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
        out RECT displayRect);

    void SetBackgroundColor(uint rgb);

    void GetBackgroundColor(out uint rgb);

    void SetPosition(DesktopWallpaperPosition position);

    void GetPosition(out DesktopWallpaperPosition position);

    void GetSlideshow(out IntPtr items);

    void SetSlideshow(IntPtr items);

    void AdvanceSlideshow(
        [MarshalAs(UnmanagedType.LPWStr)] string monitorID,
        uint direction);

    void GetSlideshowOptions(out uint options, out uint shuffle);

    void SetSlideshowOptions(uint options, uint shuffle);

    void Enable([MarshalAs(UnmanagedType.Bool)] bool enable);
}
