# MultiWall 项目指南（给 AI 助手）

## 技术栈

.NET 10 + Avalonia 12 (MVVM) + CommunityToolkit.Mvvm  
目标平台：Windows（核心功能依赖 COM `IDesktopWallpaper`）

## 目录

```
src/MultiWall/
├── Models/         MonitorInfo, WallpaperMode, AppConfig, MonitorConfig
├── Services/       WallpaperService (COM), ConfigService, LocalizationService,
│                   AutoStartService, ComInterfaces.cs
├── ViewModels/     MainWindowViewModel (核心，所有逻辑)
├── Views/          MainWindow, MonitorListView, MonitorSettingsView, SettingsView
tests/MultiWall.Tests/   xUnit + NSubstitute
```

## 命令

```bash
dotnet build MultiWall.slnx
dotnet test MultiWall.slnx
dotnet run --project src/MultiWall
```

## 架构要点

### COM 接口
`Services/ComInterfaces.cs` 定义了 `IDesktopWallpaper`（GUID `C2CF3110-...`）。  
只有 Windows 可用，Linux 编译通过但运行抛 `PlatformNotSupportedException`。

### 配置持久化
`config.json` 在 exe 同目录。`ConfigService.Load/Save` 用 `System.Text.Json`。  
`SaveConfig()` 需在用户操作后调用（BrowseSingleImage / BrowseSlideshowFolder / GoBack 等）。

### 幻灯片
全局 Timer 每秒 tick，遍历所有 `MonitorInfo`，各自用 `LastAdvanceTime` + `SlideshowInterval` 独立判断是否切换。  
`EnsureSlideshowTimer()` 在有/无活跃幻灯片时启动/停止 Timer。

### 本地化
`LocalizationService` 内存字典 → `Application.Resources.MergedDictionaries`。  
XAML 用 `{DynamicResource Key}`，代码用 `GetString(Key)`。

### 缩略图
`MonitorInfo.Thumbnail` 用 `RenderTargetBitmap` 渲染到 170×106，降低内存。

## 注意事项

1. **Bitmap 不能在被 Image 控件引用时 Dispose** → `ObjectDisposedException`。  
   Getter 内只丢弃引用（`= null`），Dispose 仅在 `MonitorInfo.Dispose()` 调用。

2. **编译绑定需要 `x:DataType`**。跨 DataContext 的命令用 `{ReflectionBinding $parent[Window].DataContext.xxx}`。

3. **初始化只做一次**：`App.OnFrameworkInitializationCompleted()` 负责 `RefreshMonitors` + `LoadAndApplyConfig`。  
   不要在 `Window.Loaded` 里重复刷新，会覆盖已加载的配置。

4. **`Preview` 只在设置页加载**（通过 ContentControl + DataTemplate 延迟创建）。列表页用 `Thumbnail`。

## TODO

- [ ] 发布时 DLL 目录优化
- [ ] 缩略图 decode 分辨率进一步降低
- [ ] 幻灯片随机/乱序播放
- [ ] 每个显示器独立排列方式（COM API 限制为全局）
