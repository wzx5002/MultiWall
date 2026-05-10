using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiWall.Services;

public static class UpdateService
{
    private const string RepoApiUrl = "https://api.github.com/repos/wzx5002/MultiWall/releases/latest";
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    static UpdateService()
    {
        _http.DefaultRequestHeaders.Add("User-Agent", "MultiWall-Update");
        _http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    }

    public static string CurrentVersion
    {
        get
        {
            var attr = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attr?.InformationalVersion ?? "0.0.0";
        }
    }

    public static bool IsSelfContained
    {
        get
        {
            var name = Assembly.GetEntryAssembly()?.GetName().Name ?? "MultiWall";
            var runtimeConfig = Path.Combine(AppContext.BaseDirectory, name + ".runtimeconfig.json");
            if (!File.Exists(runtimeConfig)) return false;
            using var doc = JsonDocument.Parse(File.ReadAllText(runtimeConfig));
            if (!doc.RootElement.TryGetProperty("runtimeOptions", out var opts)) return false;
            return !opts.TryGetProperty("framework", out _);
        }
    }

    public static async Task<UpdateResult?> CheckForUpdatesAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(RepoApiUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var latestVersion = tagName.StartsWith('v') ? tagName[1..] : tagName;

            if (!Version.TryParse(latestVersion, out var latest) ||
                !Version.TryParse(CurrentVersion, out var current))
                return null;

            if (latest <= current)
                return new UpdateResult { UpdateAvailable = false, CurrentVersion = CurrentVersion, LatestVersion = latestVersion };

            var assetPattern = IsSelfContained ? "-sc.zip" : "-fd.zip";

            string? downloadUrl = null;
            long fileSize = 0;
            foreach (var asset in root.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                if (name.EndsWith(assetPattern, StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    fileSize = asset.GetProperty("size").GetInt64();
                    break;
                }
            }

            if (downloadUrl == null)
                return null;

            var releaseNotes = root.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "";

            return new UpdateResult
            {
                UpdateAvailable = true,
                CurrentVersion = CurrentVersion,
                LatestVersion = latestVersion,
                DownloadUrl = downloadUrl,
                FileSize = fileSize,
                ReleaseNotes = releaseNotes
            };
        }
        catch (Exception ex)
        {
            return new UpdateResult
            {
                UpdateAvailable = false,
                CurrentVersion = CurrentVersion,
                LatestVersion = "",
                ErrorMessage = ex.Message
            };
        }
    }

    public static async Task<bool> DownloadAndPrepareAsync(string downloadUrl, string outDir)
    {
        try
        {
            var zipPath = Path.Combine(outDir, "update.zip");
            using var response = await _http.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var file = File.Create(zipPath);
            await stream.CopyToAsync(file);
            file.Close();

            ZipFile.ExtractToDirectory(zipPath, outDir, true);
            File.Delete(zipPath);

            CreateUpdaterScript(outDir);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void CreateUpdaterScript(string tempDir)
    {
        var appDir = AppContext.BaseDirectory;
        var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "MultiWall.exe");

        var script = $"""
@echo off
chcp 65001 >nul
echo Waiting for MultiWall to close...
:wait
timeout /t 1 /nobreak >nul
tasklist /FI "IMAGENAME eq {exeName}" 2>nul | find /I "{exeName}" >nul
if %ERRORLEVEL% equ 0 goto wait

echo Copying files...
xcopy "{tempDir}\*" "{appDir}" /E /Y /Q

echo Cleaning up...
rd /s /q "{tempDir}"

echo Starting MultiWall...
start "" "{Path.Combine(appDir, exeName)}"
""";

        File.WriteAllText(Path.Combine(tempDir, "update.bat"), script);
    }

    public static void LaunchUpdater(string tempDir)
    {
        var scriptPath = Path.Combine(tempDir, "update.bat");
        if (!File.Exists(scriptPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = tempDir
        });
    }
}

public class UpdateResult
{
    public bool UpdateAvailable { get; init; }
    public string CurrentVersion { get; init; } = "";
    public string LatestVersion { get; init; } = "";
    public string DownloadUrl { get; init; } = "";
    public long FileSize { get; init; }
    public string ReleaseNotes { get; init; } = "";
    public string ErrorMessage { get; init; } = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}
