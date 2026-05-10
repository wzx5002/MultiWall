using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace MultiWall.Services;

public static class LocalizationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> _strings = new()
    {
        ["en"] = new()
        {
            ["App.Title"] = "MultiWall - Multi-Monitor Wallpaper",
            ["Button.Refresh"] = "Refresh",
            ["Button.Start"] = "Start",
            ["Button.Stop"] = "Stop",
            ["Button.Back"] = "Back",
            ["Button.Browse"] = "Browse",
            ["Button.Configure"] = "Configure",
            ["Button.Clear"] = "Clear",
            ["Button.SlideshowFolder"] = "Select Folder",
            ["Button.SingleImage"] = "Select Image",
            ["Label.Position"] = "Position:",
            ["Label.Interval"] = "Interval:",
            ["Label.Seconds"] = "s",
            ["Label.MonitorSettings"] = "Monitor Settings",
            ["Label.NoMonitor"] = "No monitors detected",
            ["Label.SingleImage"] = "Single Image",
            ["Label.Slideshow"] = "Slideshow",
            ["Label.CurrentWallpaper"] = "Current Wallpaper:",
            ["Label.Resolution"] = "Resolution:",
            ["Label.EnableSlideshow"] = "Enable Slideshow",
            ["Label.Language"] = "Language:",
        },
        ["zh"] = new()
        {
            ["App.Title"] = "MultiWall - 多显示器壁纸",
            ["Button.Refresh"] = "刷新",
            ["Button.Start"] = "开始",
            ["Button.Stop"] = "停止",
            ["Button.Back"] = "返回",
            ["Button.Browse"] = "浏览",
            ["Button.Configure"] = "配置",
            ["Button.Clear"] = "清除",
            ["Button.SlideshowFolder"] = "选择文件夹",
            ["Button.SingleImage"] = "选择图片",
            ["Label.Position"] = "排列方式:",
            ["Label.Interval"] = "间隔:",
            ["Label.Seconds"] = "秒",
            ["Label.MonitorSettings"] = "显示器设置",
            ["Label.NoMonitor"] = "未检测到显示器",
            ["Label.SingleImage"] = "单张图片",
            ["Label.Slideshow"] = "幻灯片",
            ["Label.CurrentWallpaper"] = "当前壁纸:",
            ["Label.Resolution"] = "分辨率:",
            ["Label.EnableSlideshow"] = "启用幻灯片",
            ["Label.Language"] = "语言:",
        }
    };

    public static string CurrentLanguage { get; private set; } = "en";

    public static string GetString(string key)
    {
        if (Application.Current?.TryFindResource(key, out var resource) == true && resource is string str)
            return str;

        if (_strings.TryGetValue(CurrentLanguage, out var dict) &&
            dict.TryGetValue(key, out var value))
            return value;

        return key;
    }

    public static void SetLanguage(string language)
    {
        if (!_strings.ContainsKey(language))
            return;

        CurrentLanguage = language;

        if (Application.Current is not { } app)
            return;

        app.Resources.MergedDictionaries.Clear();

        var dict = new ResourceDictionary();
        var strings = _strings[language];
        foreach (var kvp in strings)
            dict.Add(kvp.Key, kvp.Value);

        app.Resources.MergedDictionaries.Add(dict);
    }
}
