using System;
using System.IO;
using Avalonia.Controls;

namespace MultiWall.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadTaskbarIcon();
    }

    private void LoadTaskbarIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "resource", "avalonia-logo.ico");
            if (File.Exists(iconPath))
            {
                using var stream = File.OpenRead(iconPath);
                Icon = new WindowIcon(stream);
            }
        }
        catch { }
    }
}
