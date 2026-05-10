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
    private MonitorInfo? _prevSelected;

    [ObservableProperty] private ObservableCollection<MonitorInfo> _monitors = [];
    [ObservableProperty] private MonitorInfo? _selectedMonitor;
    [ObservableProperty] private string _currentLanguage = "en";

    public bool IsSettingsOpen => SelectedMonitor != null;

    public MainWindowViewModel() : this(new WallpaperService()) { }

    public MainWindowViewModel(IWallpaperService wallpaperService)
    {
        _wallpaperService = wallpaperService;
    }

    partial void OnSelectedMonitorChanged(MonitorInfo? value)
    {
        if (_prevSelected != null)
            _prevSelected.PropertyChanged -= OnMonitorPropertyChanged;
        _prevSelected = value;

        OnPropertyChanged(nameof(IsSettingsOpen));

        if (value != null)
        {
            _wallpaperService.SetPosition(value.Position);
            ReleaseOtherPreviews(value);
            value.PropertyChanged += OnMonitorPropertyChanged;
        }
    }

    partial void OnCurrentLanguageChanged(string value)
    {
        LocalizationService.SetLanguage(value);
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
        SelectedMonitor?.ReleaseAllPreviews();
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
        EnsureSlideshowTimer();
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
        SelectedMonitor.LastAdvanceTime = DateTime.MinValue;
        SelectedMonitor.Mode = WallpaperMode.Slideshow;
        _wallpaperService.SetWallpaper(SelectedMonitor.DevicePath, images[0]);
        SelectedMonitor.WallpaperPath = images[0];
        EnsureSlideshowTimer();
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
        EnsureSlideshowTimer();
    }

    // -- Per-monitor slideshow toggle --

    [RelayCommand]
    private void ToggleMonitorSlideshow()
    {
        if (SelectedMonitor == null) return;
        SelectedMonitor.IsSlideshowRunning = !SelectedMonitor.IsSlideshowRunning;
        EnsureSlideshowTimer();
    }

    private void OnMonitorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MonitorInfo.Position) && sender == SelectedMonitor)
            _wallpaperService.SetPosition(SelectedMonitor!.Position);
    }

    // -- Slideshow timer --

    private void EnsureSlideshowTimer()
    {
        var anyActive = Monitors.Any(m =>
            m.Mode == WallpaperMode.Slideshow && m.SlideshowImages.Count > 0 && m.IsSlideshowRunning);

        if (anyActive)
            StartSlideshowTimer();
        else
            StopSlideshowTimer();
    }

    private void StartSlideshowTimer()
    {
        if (_slideshowTimer != null) return;
        _slideshowTimer = new Timer(1000);
        _slideshowTimer.Elapsed += OnSlideshowTick;
        _slideshowTimer.AutoReset = true;
        _slideshowTimer.Start();
    }

    private void StopSlideshowTimer()
    {
        if (_slideshowTimer != null)
        {
            _slideshowTimer.Stop();
            _slideshowTimer.Dispose();
            _slideshowTimer = null;
        }
    }

    private void OnSlideshowTick(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.UtcNow;
        var anyActive = false;

        foreach (var monitor in Monitors)
        {
            if (monitor.Mode != WallpaperMode.Slideshow || monitor.SlideshowImages.Count == 0)
                continue;
            if (!monitor.IsSlideshowRunning)
                { anyActive = true; continue; }

            anyActive = true;
            var interval = monitor.SlideshowInterval > 0 ? monitor.SlideshowInterval : 60;
            if ((now - monitor.LastAdvanceTime).TotalSeconds < interval)
                continue;

            monitor.LastAdvanceTime = now;
            monitor.CurrentSlideshowIndex = (monitor.CurrentSlideshowIndex + 1)
                % monitor.SlideshowImages.Count;

            var imagePath = monitor.SlideshowImages[monitor.CurrentSlideshowIndex];
            _wallpaperService.SetWallpaper(monitor.DevicePath, imagePath);

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                monitor.WallpaperPath = imagePath;
            });
        }

        if (!anyActive) StopSlideshowTimer();
    }

    private void ReleaseOtherPreviews(MonitorInfo active)
    {
        foreach (var m in Monitors)
        {
            if (m != active)
                m.ReleaseAllPreviews();
        }
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
