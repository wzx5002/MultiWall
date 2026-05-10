using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MultiWall.Models;
using MultiWall.Services;

namespace MultiWall.Views;

public partial class SettingsView : UserControl
{
    private UpdateResult? _updateResult;

    public SettingsView()
    {
        InitializeComponent();
    }

    public SettingsView(AppConfig config) : this()
    {
        DataContext = config;
        VersionLabel.Text = "v" + UpdateService.CurrentVersion;
    }

    private async void OnCheckUpdateClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CheckUpdateBtn.IsEnabled = false;
        CheckUpdateBtn.Content = "...";
        StatusLabel.Text = "";

        _updateResult = await UpdateService.CheckForUpdatesAsync();

        CheckUpdateBtn.IsEnabled = true;
        CheckUpdateBtn.Content = LocalizationService.GetString("Button.CheckUpdate");

        if (_updateResult == null)
        {
            StatusLabel.Text = LocalizationService.GetString("Label.UpToDate");
            return;
        }

        if (!_updateResult.UpdateAvailable)
        {
            StatusLabel.Text = LocalizationService.GetString("Label.UpToDate");
            return;
        }

        StatusLabel.Text = string.Format(
            LocalizationService.GetString("Label.UpdateAvailable"),
            _updateResult.LatestVersion, _updateResult.CurrentVersion);

        CheckUpdateBtn.Content = "v" + _updateResult.LatestVersion + " \u2193";
        CheckUpdateBtn.Click -= OnCheckUpdateClick;
        CheckUpdateBtn.Click += OnDownloadClick;
    }

    private async void OnDownloadClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_updateResult == null) return;

        CheckUpdateBtn.IsEnabled = false;
        CheckUpdateBtn.Content = LocalizationService.GetString("Label.Downloading");
        StatusLabel.Text = "";

        var tempDir = Path.Combine(Path.GetTempPath(), "MultiWallUpdate_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var ok = await Task.Run(() =>
            UpdateService.DownloadAndPrepareAsync(_updateResult.DownloadUrl, tempDir));

        if (!ok)
        {
            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = LocalizationService.GetString("Button.CheckUpdate");
            StatusLabel.Text = "Download failed";
            return;
        }

        StatusLabel.Text = string.Format(
            LocalizationService.GetString("Label.Updating"),
            _updateResult.LatestVersion);

        await Task.Delay(500);

        UpdateService.LaunchUpdater(tempDir);

        await Task.Delay(300);
        Environment.Exit(0);
    }
}
