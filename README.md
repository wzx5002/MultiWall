# MultiWall

Windows 多显示器独立壁纸设置工具。

## 技术栈

.NET 10 + Avalonia 12 (MVVM) + CommunityToolkit.Mvvm
目标平台：Windows（核心功能依赖 COM `IDesktopWallpaper`）

## 编译

```bash
dotnet build MultiWall.slnx
dotnet test MultiWall.slnx
dotnet run --project src/MultiWall
```

## 发布

GitHub Actions 在推送 tag（`v*`）时自动构建并发布两种版本：

| 版本 | 下载 | 说明 |
|------|------|------|
| **framework-dependent** | `MultiWall-win-x64-fd.zip` | 需安装 .NET 10 Runtime |
| **self-contained** | `MultiWall-win-x64-sc.zip` | 无需安装运行时 |

## 发布目录结构

```
publish/
├── MultiWall.exe              # 入口
├── MultiWall.dll / *.dll      # 主程序 + 所有依赖 DLL
├── resource/                  # 外部资源
│   ├── avalonia-logo.ico      # 托盘图标
│   └── Languages/
│       ├── en.axaml           # 英文字符串
│       └── zh.axaml           # 中文字符串
└── MultiWall.runtimeconfig.json / .deps.json
```

> 托管 DLL 保留在根目录是因为 .NET 的 TPA 机制不支持在 framework-dependent 部署下从子目录加载托管程序集。
