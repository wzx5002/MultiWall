using Avalonia.Controls;
using Avalonia.Input;
using MultiWall.Models;
using MultiWall.Services;
using MultiWall.ViewModels;

namespace MultiWall.Views;

public partial class MonitorListView : UserControl
{
    public MonitorListView()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        Logger.Info("ContextMenu", $"PointerPressed: RightButton={point.Properties.IsRightButtonPressed}, sender={sender?.GetType().Name}");

        if (!point.Properties.IsRightButtonPressed)
        {
            Logger.Info("ContextMenu", "Skipped: not right button");
            return;
        }

        if (sender is not Control control)
        {
            Logger.Info("ContextMenu", $"Skipped: sender {sender?.GetType().Name} is not Control");
            return;
        }

        Logger.Info("ContextMenu", $"DataContext type: {control.DataContext?.GetType().Name}");

        if (control.DataContext is not MonitorInfo monitor)
        {
            Logger.Info("ContextMenu", $"Skipped: DataContext is {control.DataContext?.GetType().Name ?? "null"}, not MonitorInfo");
            return;
        }

        if (!monitor.IsSlideshow)
        {
            Logger.Info("ContextMenu", $"Skipped: monitor {monitor.DisplayName} not in slideshow mode");
            return;
        }

        Logger.Info("ContextMenu", $"Opening context menu for {monitor.DisplayName}");

        var menu = new ContextMenu();
        var item = new MenuItem
        {
            Header = LocalizationService.GetString("Label.NextWallpaper")
        };
        item.Click += (_, _) =>
        {
            Logger.Info("ContextMenu", $"NextWallpaper clicked for {monitor.DisplayName}");
            if (VisualRoot is Window w && w.DataContext is MainWindowViewModel vm)
                vm.NextWallpaperCommand.Execute(monitor);
        };
        menu.Items.Add(item);
        menu.Open(control);

        e.Handled = true;
    }
}
