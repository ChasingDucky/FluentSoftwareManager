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

2. **方式一：使用 PowerShell 脚本（推荐）**
   ```powershell
   # 构建项目
   .\build.ps1

   # 运行项目（需要管理员权限）
   .\run.ps1

   # 发布项目
   .\publish.ps1
   ```

3. **方式二：使用 Visual Studio 2022**
   - 需要安装 "Windows 应用程序开发" 工作负载
   - 以管理员身份运行 Visual Studio
   - 打开 `FluentSoftwareManager.sln`
   - 按 F5 开始调试

4. **方式三：使用 dotnet CLI**
   ```powershell
   # 还原依赖
   dotnet restore

   # 构建
   dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

   # 运行（需要管理员权限）
   dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

   # 发布
   dotnet publish -c Release -r win-x64 -p:Platform=x64 -o ./publish
   ```

**注意**: 应用程序需要管理员权限来执行安装和卸载操作。

📖 详细的构建和调试指南请查看 [BUILD.md](BUILD.md) 和 [QUICK_START.md](QUICK_START.md)

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

## 日志和错误处理

### 日志位置

应用程序会自动记录详细的日志信息，帮助诊断问题：

```
%LOCALAPPDATA%\FluentSoftwareManager\Logs\
```

完整路径示例：
```
C:\Users\YourName\AppData\Local\FluentSoftwareManager\Logs\app20241116.log
```

### 查看日志

```powershell
# 打开日志文件夹
explorer %LOCALAPPDATA%\FluentSoftwareManager\Logs

# 查看最新日志
notepad %LOCALAPPDATA%\FluentSoftwareManager\Logs\app*.log

# 实时监控日志
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" -Wait -Tail 10
```

### 崩溃报告

如果应用崩溃，会自动生成崩溃报告：
```
%LOCALAPPDATA%\FluentSoftwareManager\Logs\crash-<timestamp>.txt
```

详细信息请参阅 [LOGGING.md](LOGGING.md)

## 故障排除

### 问题 1：应用启动后黑屏或立即关闭

**可能原因**：
- Winget 未安装或不可用
- 缺少 Windows App SDK 运行时
- 缺少 .NET 8.0 Desktop Runtime

**解决方案**：

1. **检查日志文件**：
   ```powershell
   notepad %LOCALAPPDATA%\FluentSoftwareManager\Logs\app*.log
   ```

2. **安装必需组件**：
   ```powershell
   # 安装 winget（如果没有）
   winget --version
   # 如果失败，从 Microsoft Store 安装 "应用安装程序"

   # 安装 Windows App SDK Runtime
   winget install Microsoft.WindowsAppRuntime.1.5

   # 安装 .NET Desktop Runtime
   winget install Microsoft.DotNet.DesktopRuntime.8
   ```

3. **以管理员身份运行**：
   - 右键点击应用 -> "以管理员身份运行"

### 问题 2：PowerShell 脚本无法运行

**错误信息**：
```
.\build.ps1 : The term '.\build.ps1' is not recognized...
```

**解决方案**：

选择以下任一方式：

**方式 1**：设置执行策略
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\build.ps1
```

**方式 2**：使用批处理脚本
```batch
build.bat
run.bat
```

**方式 3**：直接使用 dotnet CLI
```powershell
dotnet build FluentSoftwareManager\FluentSoftwareManager.csproj -c Debug -p:Platform=x64
dotnet run --project FluentSoftwareManager\FluentSoftwareManager.csproj -c Debug -p:Platform=x64
```

### 问题 3：找不到 dotnet 命令

**解决方案**：
```powershell
# 安装 .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8

# 验证安装
dotnet --version
```

### 问题 4：构建警告 NETSDK1206

**警告信息**：
```
warning NETSDK1206: Found version-specific or distribution-specific runtime identifier(s)
```

**说明**：这是一个无害的警告，不影响程序运行。可以忽略或通过在项目文件中添加以下内容来消除：

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);NETSDK1206</NoWarn>
</PropertyGroup>
```

### 问题 5：需要管理员权限

**错误信息**：
```
The requested operation requires elevation.
```

**解决方案**：

- **方式 1**：以管理员身份运行 PowerShell/命令提示符
- **方式 2**：右键点击已编译的 EXE -> "以管理员身份运行"

### 获取诊断信息

如果问题仍然存在，请收集以下信息：

```powershell
# 1. 查看日志
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" -Tail 50

# 2. 检查系统信息
dotnet --info
winget --version
Get-WmiObject Win32_OperatingSystem | Select-Object Caption, Version

# 3. 检查应用事件日志
Get-EventLog -LogName Application -Newest 10 -EntryType Error |
    Where-Object {$_.Source -like "*FluentSoftwareManager*" -or $_.Source -like "*.NET Runtime*"}
```

然后将信息提交到 GitHub Issues。

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
