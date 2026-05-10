using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiWall.Models;
using MultiWall.Services;

namespace MultiWall.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IWallpaperService _wallpaperService;
    private Timer? _slideshowTimer;

    [ObservableProperty] private ObservableCollection<MonitorInfo> _monitors = [];
    [ObservableProperty] private MonitorInfo? _selectedMonitor;
    [ObservableProperty] private DesktopWallpaperPosition _position = DesktopWallpaperPosition.Fill;
    [ObservableProperty] private int _slideshowInterval = 60;
    [ObservableProperty] private bool _isSlideshowRunning;
    [ObservableProperty] private string _currentLanguage = "en";

    public bool IsSettingsOpen => SelectedMonitor != null;

    public string ToggleButtonText => IsSlideshowRunning
        ? LocalizationService.GetString("Button.Stop")
        : LocalizationService.GetString("Button.Start");

    public static DesktopWallpaperPosition[] AvailablePositions { get; } =
        Enum.GetValues<DesktopWallpaperPosition>();

    public MainWindowViewModel() : this(new WallpaperService()) { }

    public MainWindowViewModel(IWallpaperService wallpaperService)
    {
        _wallpaperService = wallpaperService;
    }

    partial void OnPositionChanged(DesktopWallpaperPosition value) =>
        _wallpaperService.SetPosition(value);

    partial void OnIsSlideshowRunningChanged(bool value) =>
        OnPropertyChanged(nameof(ToggleButtonText));

    partial void OnSlideshowIntervalChanged(int value)
    {
        if (value < 1) SlideshowInterval = 1;
        if (IsSlideshowRunning) RestartSlideshow();
    }

    partial void OnSelectedMonitorChanged(MonitorInfo? value) =>
        OnPropertyChanged(nameof(IsSettingsOpen));

    partial void OnCurrentLanguageChanged(string value)
    {
        LocalizationService.SetLanguage(value);
        OnPropertyChanged(nameof(ToggleButtonText));
        foreach (var m in Monitors)
            m.RefreshDisplayName();
    }

    // -- Monitor list --

    [RelayCommand]
    private void RefreshMonitors()
    {
        var monitors = _wallpaperService.GetMonitors();
        foreach (var m in monitors)
            m.RefreshDisplayName();
        Monitors = new ObservableCollection<MonitorInfo>(monitors);
        Position = _wallpaperService.GetPosition();
    }

    // -- Navigation --

    [RelayCommand]
    private void NavigateToSettings(MonitorInfo? monitor)
    {
        SelectedMonitor = monitor;
    }

    [RelayCommand]
    private void GoBack()
    {
        SelectedMonitor?.ReleasePreview();
        SelectedMonitor = null;
    }

    // -- Single image --

    [RelayCommand]
    private async Task BrowseSingleImage()
    {
        if (SelectedMonitor == null) return;
        var window = GetMainWindow();
        if (window == null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Wallpaper Image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Images")
                {
                    Patterns = ["*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp"]
                }
            ]
        });

        if (files.Count == 0) return;

        var path = files[0].Path.LocalPath;
        _wallpaperService.SetWallpaper(SelectedMonitor.DevicePath, path);
        SelectedMonitor.WallpaperPath = path;
        SelectedMonitor.Mode = WallpaperMode.SingleImage;
    }

    // -- Slideshow folder --

    [RelayCommand]
    private async Task BrowseSlideshowFolder()
    {
        if (SelectedMonitor == null) return;
        var window = GetMainWindow();
        if (window == null) return;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Slideshow Image Folder",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        var folderPath = folders[0].Path.LocalPath;
        var validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

        var images = Directory.GetFiles(folderPath)
            .Where(f => validExtensions.Contains(Path.GetExtension(f)))
            .OrderBy(f => f)
            .ToList();

        if (images.Count == 0) return;

        SelectedMonitor.SlideshowImages = images;
        SelectedMonitor.CurrentSlideshowIndex = 0;
        SelectedMonitor.Mode = WallpaperMode.Slideshow;
        _wallpaperService.SetWallpaper(SelectedMonitor.DevicePath, images[0]);
        SelectedMonitor.WallpaperPath = images[0];
    }

    // -- Clear --

    [RelayCommand]
    private void ClearCurrentWallpaper()
    {
        if (SelectedMonitor == null) return;
        _wallpaperService.ClearWallpaper(SelectedMonitor.DevicePath);
        SelectedMonitor.WallpaperPath = string.Empty;
        SelectedMonitor.Mode = WallpaperMode.SingleImage;
        SelectedMonitor.SlideshowImages = [];
    }

    // -- Slideshow timer --

    [RelayCommand]
    private void ToggleSlideshow()
    {
        if (IsSlideshowRunning) StopSlideshow();
        else StartSlideshow();
    }

    private void StartSlideshow()
    {
        StopSlideshow();
        _slideshowTimer = new Timer(SlideshowInterval * 1000);
        _slideshowTimer.Elapsed += OnSlideshowTick;
        _slideshowTimer.AutoReset = true;
        _slideshowTimer.Start();
        IsSlideshowRunning = true;
    }

    private void StopSlideshow()
    {
        if (_slideshowTimer != null)
        {
            _slideshowTimer.Stop();
            _slideshowTimer.Dispose();
            _slideshowTimer = null;
        }
        IsSlideshowRunning = false;
    }

    private void RestartSlideshow()
    {
        if (IsSlideshowRunning) StartSlideshow();
    }

    private void OnSlideshowTick(object? sender, ElapsedEventArgs e)
    {
        foreach (var monitor in Monitors)
        {
            if (monitor.Mode != WallpaperMode.Slideshow || monitor.SlideshowImages.Count == 0) continue;

            monitor.CurrentSlideshowIndex = (monitor.CurrentSlideshowIndex + 1)
                % monitor.SlideshowImages.Count;

            var imagePath = monitor.SlideshowImages[monitor.CurrentSlideshowIndex];
            _wallpaperService.SetWallpaper(monitor.DevicePath, imagePath);

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                monitor.WallpaperPath = imagePath;
            });
        }
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
