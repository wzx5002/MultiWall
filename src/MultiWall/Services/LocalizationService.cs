using System;
using System.IO;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;

namespace MultiWall.Services;

public static class LocalizationService
{
    private static readonly XNamespace AvaloniaNs = "https://github.com/avaloniaui";
    private static readonly XNamespace XNs = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly XNamespace SysNs = "clr-namespace:System;assembly=System.Runtime";

    public static string CurrentLanguage { get; private set; } = "en";

    public static string GetString(string key)
    {
        if (Application.Current?.TryFindResource(key, out var resource) == true && resource is string str)
            return str;
        return key;
    }

    public static void SetLanguage(string language)
    {
        CurrentLanguage = language;

        if (Application.Current is not { } app)
            return;

        app.Resources.MergedDictionaries.Clear();

        var filePath = Path.Combine(AppContext.BaseDirectory, "resource", "Languages", language + ".axaml");
        Logger.Info("Loc", $"Loading {filePath}");
        if (!File.Exists(filePath))
        {
            Logger.Error("Loc", $"File not found: {filePath}");
            return;
        }

        try
        {
            var xdoc = XDocument.Load(filePath);

            if (xdoc.Root == null)
                return;

            var dict = new ResourceDictionary();

            foreach (var elem in xdoc.Root.Elements(SysNs + "String"))
            {
                var key = (string?)elem.Attribute(XNs + "Key");
                var value = (string?)elem;
                if (key != null && value != null)
                    dict.Add(key, value);
            }

            app.Resources.MergedDictionaries.Add(dict);
            Logger.Info("Loc", $"Loaded {language} with {dict.Count} entries");
        }
        catch (Exception ex)
        {
            Logger.Error("Loc", ex);
        }
    }
}
