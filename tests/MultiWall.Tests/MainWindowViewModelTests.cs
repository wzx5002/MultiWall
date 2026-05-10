using MultiWall.Models;
using MultiWall.Services;
using MultiWall.ViewModels;
using NSubstitute;
using Xunit;
using static MultiWall.Models.WallpaperMode;

namespace MultiWall.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void RefreshMonitors_PopulatesMonitorList()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        wallpaper.GetMonitors().Returns([
            new MonitorInfo
            {
                Index = 0, DevicePath = @"\\?\DISPLAY#TEST#123",
                Left = 0, Top = 0, Right = 1920, Bottom = 1080,
                WallpaperPath = @"C:\wallpaper.jpg"
            }
        ]);

        var vm = new MainWindowViewModel(wallpaper);
        vm.RefreshMonitorsCommand.Execute(null);

        Assert.Single(vm.Monitors);
        Assert.Equal(@"\\?\DISPLAY#TEST#123", vm.Monitors[0].DevicePath);
        Assert.Equal(1920, vm.Monitors[0].Width);
        Assert.Equal(1080, vm.Monitors[0].Height);
    }

    [Fact]
    public void NavigateToSettings_SetsSelectedMonitor()
    {
        var vm = new MainWindowViewModel(Substitute.For<IWallpaperService>());
        var monitor = new MonitorInfo { Index = 0, DevicePath = "MONITOR1" };

        Assert.False(vm.IsSettingsOpen);
        vm.NavigateToSettingsCommand.Execute(monitor);
        Assert.True(vm.IsSettingsOpen);
        Assert.Equal(monitor, vm.SelectedMonitor);
    }

    [Fact]
    public void GoBack_ClearsSelectedMonitor()
    {
        var vm = new MainWindowViewModel(Substitute.For<IWallpaperService>());
        vm.NavigateToSettingsCommand.Execute(new MonitorInfo { Index = 0, DevicePath = "M1" });
        Assert.True(vm.IsSettingsOpen);
        vm.GoBackCommand.Execute(null);
        Assert.False(vm.IsSettingsOpen);
        Assert.Null(vm.SelectedMonitor);
    }

    [Fact]
    public void ClearCurrentWallpaper_ClearsSelectedMonitor()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        var vm = new MainWindowViewModel(wallpaper);
        var monitor = new MonitorInfo
        {
            Index = 0, DevicePath = "MONITOR1",
            WallpaperPath = @"C:\img.jpg",
            Mode = Slideshow,
            SlideshowImages = [@"C:\img1.jpg", @"C:\img2.jpg"]
        };

        vm.NavigateToSettingsCommand.Execute(monitor);
        vm.ClearCurrentWallpaperCommand.Execute(null);

        wallpaper.Received(1).ClearWallpaper("MONITOR1");
        Assert.Empty(monitor.WallpaperPath);
        Assert.Equal(SingleImage, monitor.Mode);
        Assert.Empty(monitor.SlideshowImages);
    }

    [Fact]
    public void ToggleMonitorSlideshow_FlipsIsSlideshowRunning()
    {
        var vm = new MainWindowViewModel(Substitute.For<IWallpaperService>());
        var monitor = new MonitorInfo { Index = 0, DevicePath = "M1", IsSlideshowRunning = true };

        vm.NavigateToSettingsCommand.Execute(monitor);
        Assert.True(monitor.IsSlideshowRunning);

        vm.ToggleMonitorSlideshowCommand.Execute(null);
        Assert.False(monitor.IsSlideshowRunning);
    }

    [Fact]
    public void PositionChanged_CallsService()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        var vm = new MainWindowViewModel(wallpaper);
        var monitor = new MonitorInfo { Index = 0, DevicePath = "M1", Position = DesktopWallpaperPosition.Fill };

        vm.NavigateToSettingsCommand.Execute(monitor);
        wallpaper.Received(1).SetPosition(DesktopWallpaperPosition.Fill);

        monitor.Position = DesktopWallpaperPosition.Span;
        wallpaper.Received(1).SetPosition(DesktopWallpaperPosition.Span);
    }

    [Fact]
    public void LanguageChanged_UpdatesDisplayNames()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        wallpaper.GetMonitors().Returns([new MonitorInfo { Index = 0, DevicePath = "M1" }]);

        var vm = new MainWindowViewModel(wallpaper);
        vm.RefreshMonitorsCommand.Execute(null);

        Assert.Equal("Monitor 1", vm.Monitors[0].DisplayName);
        Assert.Equal("en", vm.CurrentLanguage);

        vm.CurrentLanguage = "zh";
        Assert.Equal("显示器 1", vm.Monitors[0].DisplayName);
    }
}
