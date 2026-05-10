using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MultiWall.Services;

namespace MultiWall.Models;

public partial class MonitorInfo : ObservableObject, IDisposable
{
    [ObservableProperty] private int _index;
    [ObservableProperty] private string _devicePath = string.Empty;
    [ObservableProperty] private int _left;
    [ObservableProperty] private int _top;
    [ObservableProperty] private int _right;
    [ObservableProperty] private int _bottom;
    [ObservableProperty] private string _wallpaperPath = string.Empty;
    [ObservableProperty] private WallpaperMode _mode = WallpaperMode.SingleImage;
    [ObservableProperty] private List<string> _slideshowImages = [];
    [ObservableProperty] private int _currentSlideshowIndex;

    private Bitmap? _cachedPreview;
    private string? _cachedPreviewPath;

    public string DisplayName => LocalizationService.CurrentLanguage == "zh"
        ? $"显示器 {Index + 1}"
        : $"Monitor {Index + 1}";

    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public string BoundsText => $"{Width} x {Height}  ({Left}, {Top})";

    public bool IsSlideshow => Mode == WallpaperMode.Slideshow;

    public int ModeIndex
    {
        get => (int)Mode;
        set => Mode = (WallpaperMode)value;
    }

    public Bitmap? Preview
    {
        get
        {
            if (string.IsNullOrEmpty(WallpaperPath) || !File.Exists(WallpaperPath))
            {
                ReleaseCachedPreview();
                return null;
            }

            if (_cachedPreviewPath == WallpaperPath && _cachedPreview != null)
                return _cachedPreview;

            ReleaseCachedPreview();

            try
            {
                _cachedPreview = new Bitmap(WallpaperPath);
                _cachedPreviewPath = WallpaperPath;
                return _cachedPreview;
            }
            catch
            {
                return null;
            }
        }
    }

    public void RefreshDisplayName() => OnPropertyChanged(nameof(DisplayName));

    public void ReleasePreview() => ReleaseCachedPreview();

    private void ReleaseCachedPreview()
    {
        if (_cachedPreview != null)
        {
            _cachedPreview.Dispose();
            _cachedPreview = null;
            _cachedPreviewPath = null;
        }
    }

    partial void OnModeChanged(WallpaperMode value)
    {
        OnPropertyChanged(nameof(ModeIndex));
        OnPropertyChanged(nameof(IsSlideshow));
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

    public void Dispose()
    {
        ReleaseCachedPreview();
    }
}
