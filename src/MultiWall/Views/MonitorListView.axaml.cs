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
        if (!point.Properties.IsRightButtonPressed) return;
        if (sender is not Control control) return;
        if (control.DataContext is not MonitorInfo monitor || !monitor.IsSlideshow) return;

        var menu = new ContextMenu();
        var item = new MenuItem
        {
            Header = LocalizationService.GetString("Label.NextWallpaper")
        };
        item.Click += (_, _) =>
        {
            if (VisualRoot is Window w && w.DataContext is MainWindowViewModel vm)
                vm.NextWallpaperCommand.Execute(monitor);
        };
        menu.Items.Add(item);
        menu.Open(control);

        e.Handled = true;
    }
}
