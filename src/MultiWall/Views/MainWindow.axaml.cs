using Avalonia.Controls;
using MultiWall.ViewModels;

namespace MultiWall.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.RefreshMonitorsCommand.Execute(null);
    }
}
