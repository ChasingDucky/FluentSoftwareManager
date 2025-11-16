# Fluent Software Manager

一个基于 Microsoft Fluent Design System 设计的现代 Windows 软件包管理器，使用 WinUI 3 构建，集成 Windows Package Manager (winget) 功能。

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## 功能特性

- **🔍 软件浏览** - 搜索和浏览 winget 仓库中的软件包
- **📦 软件安装** - 一键安装所需软件
- **🗑️ 软件卸载** - 管理和卸载已安装的软件
- **🔄 软件更新** - 检查并更新已安装的软件
- **🎨 Fluent Design** - 采用 Microsoft Fluent Design System 设计语言
- **⚡ 现代化界面** - 基于 WinUI 3 的流畅、美观的用户界面

## 系统要求

- Windows 10 版本 1809 (Build 17763) 或更高版本
- Windows 11（推荐）
- .NET 8.0 运行时
- Windows Package Manager (winget)

## 安装

### 先决条件

1. 安装 Windows Package Manager (winget)
   - Windows 11 系统已预装
   - Windows 10 用户可从 Microsoft Store 安装 "应用安装程序"

2. 安装 .NET 8.0 Runtime
   ```powershell
   winget install Microsoft.DotNet.Runtime.8
   ```

### 从源码构建

1. 克隆仓库
   ```bash
   git clone https://github.com/yourusername/FluentSoftwareManager.git
   cd FluentSoftwareManager
   ```

2. 使用 Visual Studio 2022 打开解决方案
   - 需要安装 "Windows 应用程序开发" 工作负载
   - 需要安装 ".NET 桌面开发" 工作负载

3. 构建并运行
   - 按 F5 或点击"启动"按钮

或使用命令行：
```powershell
dotnet build
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj
```

## 使用方法

### 浏览软件

1. 启动应用程序
2. 在左侧导航栏选择 "Browse"
3. 使用搜索框搜索软件包
4. 点击软件查看详细信息
5. 点击 "Install" 按钮安装软件

### 管理已安装软件

1. 在左侧导航栏选择 "Installed"
2. 查看所有已安装的软件列表
3. 选择要卸载的软件
4. 点击 "Uninstall" 按钮

### 更新软件

1. 在左侧导航栏选择 "Updates"
2. 查看所有可更新的软件
3. 点击单个软件的 "Update" 按钮更新
4. 或点击 "Update All" 更新所有软件

## 项目结构

```
FluentSoftwareManager/
├── FluentSoftwareManager/
│   ├── Models/              # 数据模型
│   │   └── Package.cs
│   ├── Services/            # 服务层
│   │   └── WingetService.cs
│   ├── ViewModels/          # 视图模型
│   │   ├── BrowseViewModel.cs
│   │   ├── InstalledViewModel.cs
│   │   └── UpdatesViewModel.cs
│   ├── Views/               # 视图
│   │   ├── BrowsePage.xaml
│   │   ├── InstalledPage.xaml
│   │   ├── UpdatesPage.xaml
│   │   └── SettingsPage.xaml
│   ├── Converters/          # XAML 转换器
│   │   └── BoolToVisibilityConverter.cs
│   ├── App.xaml            # 应用程序入口
│   └── MainWindow.xaml     # 主窗口
└── FluentSoftwareManager.sln
```

## 技术栈

- **框架**: .NET 8.0
- **UI 框架**: WinUI 3
- **MVVM 工具包**: CommunityToolkit.Mvvm
- **包管理器**: Windows Package Manager (winget)
- **设计语言**: Microsoft Fluent Design System

## 架构设计

本项目采用 MVVM (Model-View-ViewModel) 架构模式：

- **Model**: 定义数据结构（Package）
- **View**: XAML 视图文件，使用 Fluent Design 组件
- **ViewModel**: 处理业务逻辑和数据绑定
- **Service**: WingetService 封装 winget 命令行调用

## 特性说明

### Fluent Design Elements

- **Acrylic Material**: 半透明背景效果
- **Navigation View**: 导航视图控件
- **Card Layout**: 卡片式布局
- **Icons**: Fluent 图标系统
- **Color System**: 自适应颜色系统

### Winget 集成

应用程序通过命令行调用 winget，支持以下操作：
- `winget search` - 搜索软件包
- `winget list` - 列出已安装软件
- `winget install` - 安装软件
- `winget uninstall` - 卸载软件
- `winget upgrade` - 更新软件

## 权限说明

本应用程序需要管理员权限以执行软件的安装和卸载操作。

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request！

## 待办事项

- [ ] 添加软件详情对话框
- [ ] 支持批量安装
- [ ] 添加下载进度显示
- [ ] 支持多语言
- [ ] 添加软件分类浏览
- [ ] 支持自定义软件源
- [ ] 添加软件评分和评论

## 联系方式

如有问题或建议，请提交 Issue。

---

Made with ❤️ using WinUI 3 and Fluent Design
