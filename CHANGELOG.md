# Changelog

## v1.1.1

### 新增
- 任务栏图标（`MainWindow.Icon` 加载 `resource/avalonia-logo.ico`）
- 幻灯片随机顺序播放（`SlideshowShuffle` checkbox）

### 修复
- 设置窗口高度不足导致更新控件不可见（240 → 360）

## v1.1.0

### 新增
- 自动检查更新（GitHub Releases API → 下载 → 替换 → 重启）
- 设置页面底部显示当前版本号 + 检查更新按钮
- 自动识别 fd/sc 部署类型下载对应 zip
- `update.bat` 脚本实现无感替换

## 2026-05-10

### 新增
- 中英双语支持（LocalizationService 从外部 axaml 文件加载）
- 系统托盘图标 + 右键菜单（Show / 显示器列表 / Settings / Exit）
- 开机自启（注册表 Run key）
- 配置持久化（config.json，含每显示器模式和图片路径）
- 设置页面（托盘行为 / 开机自启 / 语言）
- 托盘菜单动态显示各显示器，点击直达配置页
- GitHub Actions 双版本发布（framework-dependent + self-contained）
- 发布目录 resource/ 子目录存放图标和语言文件

### 修复
- 启动时空 UI：语言资源初始化为空
- 幻灯片 Bitmap 内存泄漏
- ObjectDisposedException：返回/切换幻灯片时释放了 UI 仍在引用的 Bitmap
- 托盘图标无图标：改用文件加载 resource/avalonia-logo.ico
- 重启后配置不恢复：MainWindow.Loaded 重复调用 RefreshMonitors 覆盖了已加载配置
- 工具栏精简：全局排列/间隔/启停移到每显示器设置页
- Release 权限缺失：workflow 添加 `contents: write`

### 优化
- 列表页缩略图用 RenderTargetBitmap 170×106（~70KB），不再加载原始分辨率
- 幻灯片自动启停：有活跃幻灯片时启动计时器，无则停止
- 配置保存时机：每次修改后立即写入，不再仅退出时保存
- 语言文件从嵌入式改为外部 resource/ 目录，便于修改

### 已知限制
- 托管 DLL 无法移入子目录：.NET 的 TPA 机制在 framework-dependent 部署下不支持从子目录加载托管程序集
