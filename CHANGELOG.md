# Changelog

## 2026-05-10

### 新增
- 中英双语支持（LocalizationService 内存字典）
- 系统托盘图标 + 右键菜单（Show / 显示器列表 / Settings / Exit）
- 开机自启（注册表 Run key）
- 配置持久化（config.json，含每显示器模式和图片路径）
- 设置页面（托盘行为 / 开机自启 / 语言）
- 托盘菜单动态显示各显示器，点击直达配置页

### 修复
- 启动时空 UI：语言资源初始化为空
- 幻灯片 Bitmap 内存泄漏
- ObjectDisposedException：返回/切换幻灯片时释放了 UI 仍在引用的 Bitmap
- 托盘图标无图标：改用 AssetLoader 加载 .ico 资源
- 重启后配置不恢复：MainWindow.Loaded 重复调用 RefreshMonitors 覆盖了已加载配置
- 工具栏精简：全局排列/间隔/启停移到每显示器设置页

### 优化
- 列表页缩略图用 RenderTargetBitmap 170×106（~70KB），不再加载原始分辨率
- 幻灯片自动启停：有活跃幻灯片时启动计时器，无则停止
- 配置保存时机：每次修改后立即写入，不再仅退出时保存
