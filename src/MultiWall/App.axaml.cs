using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MultiWall.Models;
using MultiWall.Services;
using MultiWall.ViewModels;
using MultiWall.Views;

namespace MultiWall;

public partial class App : Application
{
    private TrayIcon? _trayIcon;
    private AppConfig _config = null!;
    private MainWindowViewModel? _vm;
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _config = ConfigService.Load();

            LocalizationService.SetLanguage(_config.Language);

            var wallpaperService = new WallpaperService();
            _vm = new MainWindowViewModel(wallpaperService, _config);

            desktop.MainWindow = new MainWindow
            {
                DataContext = _vm,
            };

            desktop.MainWindow.Closing += OnMainWindowClosing;
            desktop.Exit += OnExit;

            SetupTrayIcon();
            ApplyAutoStart();
            _vm.RefreshMonitorsCommand.Execute(null);
            _vm.LoadAndApplyConfig();
            RebuildTrayMenu();

            _vm.Monitors.CollectionChanged += (_, _) => RebuildTrayMenu();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupTrayIcon()
    {
        try
        {
            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon("avares://MultiWall/Assets/avalonia-logo.ico"),
                ToolTipText = "MultiWall"
            };
            _trayIcon.Clicked += (_, _) => ShowMainWindow();
            _trayIcon.Menu = new NativeMenu();
        }
        catch { }
    }

    private void RebuildTrayMenu()
    {
        if (_trayIcon == null) return;

        var menu = new NativeMenu();

        var showItem = new NativeMenuItem(LocalizationService.GetString("Tray.Show"));
        showItem.Click += (_, _) => ShowMainWindow();
        menu.Add(showItem);

        menu.Add(new NativeMenuItemSeparator());

        if (_vm != null)
        {
            foreach (var monitor in _vm.Monitors)
            {
                var monitorItem = new NativeMenuItem(monitor.DisplayName);
                var captured = monitor;
                monitorItem.Click += (_, _) => ShowAndNavigateTo(captured);
                menu.Add(monitorItem);
            }
        }

        menu.Add(new NativeMenuItemSeparator());

        var settingsItem = new NativeMenuItem(LocalizationService.GetString("Label.Settings"));
        settingsItem.Click += (_, _) =>
        {
            ShowMainWindow();
            Dispatcher.UIThread.Post(() => _vm?.OpenSettingsCommand.Execute(null));
        };
        menu.Add(settingsItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem(LocalizationService.GetString("Tray.Exit"));
        exitItem.Click += (_, _) => ForceExit();
        menu.Add(exitItem);

        _trayIcon.Menu = menu;
    }

    private void ShowAndNavigateTo(MonitorInfo monitor)
    {
        ShowMainWindow();
        Dispatcher.UIThread.Post(() =>
        {
            _vm?.NavigateToSettingsCommand.Execute(monitor);
        });
    }

    private void ShowMainWindow()
    {
        if (_desktop?.MainWindow == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _desktop.MainWindow.Show();
            _desktop.MainWindow.BringIntoView();
            _desktop.MainWindow.Activate();
        });
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_config.MinimizeToTray)
        {
            e.Cancel = true;
            if (sender is Window w) w.Hide();
        }
    }

    private void ForceExit()
    {
        _vm?.SaveConfig();
        _trayIcon?.Dispose();
        _desktop?.Shutdown();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _vm?.SaveConfig();
        _trayIcon?.Dispose();
    }

    private void ApplyAutoStart()
    {
        if (OperatingSystem.IsWindows())
            AutoStartService.SetEnabled(_config.AutoStart);
    }

    public static void ShowSettings(AppConfig config)
    {
        var current = Application.Current;
        if (current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var settings = new SettingsView(config);
        var win = new Window
        {
            Title = LocalizationService.GetString("Label.Settings"),
            Width = 360, Height = 240,
            Content = settings,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };
        win.Closed += (_, _) =>
        {
            ConfigService.Save(config);
            if (OperatingSystem.IsWindows())
                AutoStartService.SetEnabled(config.AutoStart);
            if (desktop.MainWindow?.DataContext is MainWindowViewModel vm)
                vm.OnSettingsApplied();
        };
        win.ShowDialog(desktop.MainWindow!);
    }

    public static void RebuildTray() =>
        (Application.Current as App)?.RebuildTrayMenu();
}
