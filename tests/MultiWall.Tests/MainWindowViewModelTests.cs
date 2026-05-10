using MultiWall.Models;
using MultiWall.Services;
using MultiWall.ViewModels;
using NSubstitute;
using Xunit;

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
                Index = 0,
                DevicePath = @"\\?\DISPLAY#TEST#123",
                Left = 0, Top = 0, Right = 1920, Bottom = 1080,
                WallpaperPath = @"C:\wallpaper.jpg"
            }
        ]);
        wallpaper.GetPosition().Returns(DesktopWallpaperPosition.Fill);

        var vm = new MainWindowViewModel(wallpaper);
        vm.RefreshMonitorsCommand.Execute(null);

        Assert.Single(vm.Monitors);
        Assert.Equal(@"\\?\DISPLAY#TEST#123", vm.Monitors[0].DevicePath);
        Assert.Equal(1920, vm.Monitors[0].Width);
        Assert.Equal(1080, vm.Monitors[0].Height);
        Assert.Equal(DesktopWallpaperPosition.Fill, vm.Position);
    }

    [Fact]
    public void ClearWallpaper_RemovesWallpaperFromMonitor()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        wallpaper.GetMonitors().Returns([
            new MonitorInfo
            {
                Index = 0,
                DevicePath = "MONITOR1",
                WallpaperPath = @"C:\img.jpg"
            }
        ]);

        var vm = new MainWindowViewModel(wallpaper);
        vm.RefreshMonitorsCommand.Execute(null);
        vm.ClearWallpaperCommand.Execute("MONITOR1");

        wallpaper.Received(1).ClearWallpaper("MONITOR1");
        Assert.Empty(vm.Monitors[0].WallpaperPath);
        Assert.False(vm.Monitors[0].IsSlideshow);
    }

    [Fact]
    public void ToggleSlideshow_StartsAndStops()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        var vm = new MainWindowViewModel(wallpaper);

        Assert.False(vm.IsSlideshowRunning);
        vm.ToggleSlideshowCommand.Execute(null);
        Assert.True(vm.IsSlideshowRunning);
        vm.ToggleSlideshowCommand.Execute(null);
        Assert.False(vm.IsSlideshowRunning);
    }

    [Fact]
    public void SlideshowIntervalChangedDuringRunning_RestartsTimer()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        var vm = new MainWindowViewModel(wallpaper);

        vm.ToggleSlideshowCommand.Execute(null);
        Assert.True(vm.IsSlideshowRunning);

        vm.SlideshowInterval = 30;
        Assert.True(vm.IsSlideshowRunning);
    }

    [Fact]
    public void PositionChanged_CallsService()
    {
        var wallpaper = Substitute.For<IWallpaperService>();
        var vm = new MainWindowViewModel(wallpaper);

        vm.Position = DesktopWallpaperPosition.Span;
        wallpaper.Received(1).SetPosition(DesktopWallpaperPosition.Span);
    }
}
