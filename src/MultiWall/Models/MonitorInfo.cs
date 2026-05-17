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
    [ObservableProperty] private bool _slideshowShuffle;
    [ObservableProperty] private DesktopWallpaperPosition _position = DesktopWallpaperPosition.Fill;

    private Bitmap? _cachedPreview;
    private string? _cachedPreviewPath;
    private IImage? _cachedThumbnail;
    private string? _cachedThumbnailPath;
    internal DateTime LastAdvanceTime = DateTime.MinValue;
    internal List<int> ShuffledOrder = [];
    internal int ShuffledPosition;

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
                _cachedPreview = null;
                _cachedPreviewPath = null;
                return null;
            }
            if (_cachedPreviewPath == WallpaperPath && _cachedPreview != null)
                return _cachedPreview;

            _cachedPreview = null;
            _cachedPreviewPath = null;
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
                _cachedThumbnail = null;
                _cachedThumbnailPath = null;
                return null;
            }
            if (_cachedThumbnailPath == WallpaperPath && _cachedThumbnail != null)
                return _cachedThumbnail;

            _cachedThumbnail = null;
            _cachedThumbnailPath = null;
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

    public void ReleasePreview() { _cachedPreview = null; _cachedPreviewPath = null; }
    public void ReleaseThumbnail() { _cachedThumbnail = null; _cachedThumbnailPath = null; }
    public void ReleaseAllPreviews() { ReleasePreview(); ReleaseThumbnail(); }

    partial void OnModeChanged(WallpaperMode value)
    {
        if (value == WallpaperMode.Slideshow)
            IsSlideshowRunning = true;
        OnPropertyChanged(nameof(ModeIndex));
        OnPropertyChanged(nameof(IsSlideshow));
    }

    partial void OnIsSlideshowRunningChanged(bool value) =>
        OnPropertyChanged(nameof(SlideshowToggleText));

    partial void OnSlideshowShuffleChanged(bool value)
    {
        if (value) ShuffledOrder = [];
    }

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
        if (_cachedPreview != null) { _cachedPreview.Dispose(); _cachedPreview = null; }
        if (_cachedThumbnail is IDisposable d) d.Dispose();
        _cachedThumbnail = null;
        _cachedPreviewPath = null;
        _cachedThumbnailPath = null;
    }

    internal void RebuildShuffleOrder(Random rng)
    {
        var count = SlideshowImages.Count;
        if (count <= 1)
        {
            ShuffledOrder = [];
            ShuffledPosition = 0;
            return;
        }

        var order = new List<int>(count);
        for (var i = 0; i < count; i++) order.Add(i);

        for (var i = count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        if (ShuffledOrder.Count > 0 && order[0] == ShuffledOrder[^1] && count > 1)
        {
            var swap = rng.Next(1, count);
            (order[0], order[swap]) = (order[swap], order[0]);
        }
        else if (ShuffledOrder.Count == 0 && order[0] == CurrentSlideshowIndex && count > 1)
        {
            var swap = rng.Next(1, count);
            (order[0], order[swap]) = (order[swap], order[0]);
        }

        ShuffledOrder = order;
        ShuffledPosition = 0;
    }
}
