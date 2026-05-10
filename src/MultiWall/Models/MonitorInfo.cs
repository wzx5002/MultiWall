using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiWall.Models;

public partial class MonitorInfo : ObservableObject
{
    [ObservableProperty] private int _index;
    [ObservableProperty] private string _devicePath = string.Empty;
    [ObservableProperty] private int _left;
    [ObservableProperty] private int _top;
    [ObservableProperty] private int _right;
    [ObservableProperty] private int _bottom;
    [ObservableProperty] private string _wallpaperPath = string.Empty;
    [ObservableProperty] private bool _isSlideshow;
    [ObservableProperty] private List<string> _slideshowImages = [];
    [ObservableProperty] private int _currentSlideshowIndex;

    public string DisplayName => $"Monitor {Index + 1}";
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public string BoundsText => $"{Width} x {Height}  ({Left}, {Top})";

    public Bitmap? Preview
    {
        get
        {
            if (string.IsNullOrEmpty(WallpaperPath) || !File.Exists(WallpaperPath))
                return null;
            try
            {
                return new Bitmap(WallpaperPath);
            }
            catch
            {
                return null;
            }
        }
    }

    partial void OnIndexChanged(int value) => OnPropertyChanged(nameof(DisplayName));

    partial void OnWallpaperPathChanged(string value) => OnPropertyChanged(nameof(Preview));

    partial void OnLeftChanged(int value) => NotifyBoundsChanged();
    partial void OnTopChanged(int value) => NotifyBoundsChanged();
    partial void OnRightChanged(int value) => NotifyBoundsChanged();
    partial void OnBottomChanged(int value) => NotifyBoundsChanged();

    private void NotifyBoundsChanged()
    {
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(BoundsText));
    }
}
