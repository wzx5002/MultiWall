using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MultiWall.Services;

namespace MultiWall.Models;

public partial class MonitorInfo : ObservableObject, IDisposable
{
    private static readonly PixelSize ThumbnailSize = new(170, 106);
    private static readonly Vector Dpi = new(96, 96);

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
    [ObservableProperty] private int _slideshowInterval = 60;
    [ObservableProperty] private bool _isSlideshowRunning = true;
    [ObservableProperty] private DesktopWallpaperPosition _position = DesktopWallpaperPosition.Fill;

    private Bitmap? _cachedPreview;
    private string? _cachedPreviewPath;
    private IImage? _cachedThumbnail;
    private string? _cachedThumbnailPath;
    internal DateTime LastAdvanceTime = DateTime.MinValue;

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

    public string SlideshowToggleText => IsSlideshowRunning
        ? LocalizationService.GetString("Button.Stop")
        : LocalizationService.GetString("Button.Start");

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
            catch { return null; }
        }
    }

    public IImage? Thumbnail
    {
        get
        {
            if (string.IsNullOrEmpty(WallpaperPath) || !File.Exists(WallpaperPath))
            {
                ReleaseCachedThumbnail();
                return null;
            }
            if (_cachedThumbnailPath == WallpaperPath && _cachedThumbnail != null)
                return _cachedThumbnail;
            ReleaseCachedThumbnail();
            try
            {
                using var original = new Bitmap(WallpaperPath);
                var rt = new RenderTargetBitmap(ThumbnailSize, Dpi);
                using var ctx = rt.CreateDrawingContext();
                ctx.DrawImage(original, new Rect(0, 0, ThumbnailSize.Width, ThumbnailSize.Height));
                _cachedThumbnail = rt;
                _cachedThumbnailPath = WallpaperPath;
                return _cachedThumbnail;
            }
            catch { return null; }
        }
    }

    public void RefreshDisplayName() => OnPropertyChanged(nameof(DisplayName));

    public void ReleasePreview() => ReleaseCachedPreview();
    public void ReleaseThumbnail() => ReleaseCachedThumbnail();
    public void ReleaseAllPreviews() { ReleaseCachedPreview(); ReleaseCachedThumbnail(); }

    private void ReleaseCachedPreview()
    {
        if (_cachedPreview != null) { _cachedPreview.Dispose(); _cachedPreview = null; _cachedPreviewPath = null; }
    }

    private void ReleaseCachedThumbnail()
    {
        if (_cachedThumbnail is IDisposable d) d.Dispose();
        _cachedThumbnail = null;
        _cachedThumbnailPath = null;
    }

    partial void OnModeChanged(WallpaperMode value)
    {
        if (value == WallpaperMode.Slideshow)
            IsSlideshowRunning = true;
        OnPropertyChanged(nameof(ModeIndex));
        OnPropertyChanged(nameof(IsSlideshow));
    }

    partial void OnIsSlideshowRunningChanged(bool value) =>
        OnPropertyChanged(nameof(SlideshowToggleText));

    partial void OnIndexChanged(int value) => OnPropertyChanged(nameof(DisplayName));
    partial void OnWallpaperPathChanged(string value)
    {
        _cachedThumbnailPath = null;
        OnPropertyChanged(nameof(Preview));
        OnPropertyChanged(nameof(Thumbnail));
    }
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
        ReleaseCachedThumbnail();
    }
}
