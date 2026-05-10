using System.Collections.Generic;

namespace MultiWall.Models;

public class AppConfig
{
    public List<MonitorConfig> Monitors { get; set; } = [];
    public bool MinimizeToTray { get; set; } = true;
    public bool AutoStart { get; set; }
    public string Language { get; set; } = "en";
}
