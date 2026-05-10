using Avalonia.Controls;
using MultiWall.Models;

namespace MultiWall.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    public SettingsView(AppConfig config) : this()
    {
        DataContext = config;
    }
}
