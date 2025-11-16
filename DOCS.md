# Fluent Software Manager - 开发文档

## 项目概述

Fluent Software Manager 是一个基于 Microsoft Fluent Design System 的现代化 Windows 软件包管理器。它使用 WinUI 3 框架构建，集成了 Windows Package Manager (winget) 功能，为用户提供优雅、流畅的软件管理体验。

## 核心功能

### 1. 软件浏览（Browse）
- 搜索 winget 仓库中的软件包
- 实时搜索功能
- 显示软件名称、ID、版本信息
- 一键安装未安装的软件

### 2. 已安装软件管理（Installed）
- 列出所有已安装的软件
- 显示软件版本信息
- 一键卸载功能
- 刷新功能

### 3. 软件更新（Updates）
- 检查所有可更新的软件
- 显示当前版本和可用版本
- 单个软件更新
- 批量更新所有软件

### 4. 设置（Settings）
- 主题切换（浅色/深色/跟随系统）
- 应用信息展示
- Winget 集成说明

## 技术架构

### 技术栈

```
Frontend: WinUI 3
Framework: .NET 8.0
Pattern: MVVM (Model-View-ViewModel)
MVVM Toolkit: CommunityToolkit.Mvvm
Package Manager: Windows Package Manager (winget)
Design System: Microsoft Fluent Design System
```

### 项目结构

```
FluentSoftwareManager/
│
├── Models/                          # 数据模型层
│   └── Package.cs                   # 软件包数据模型
│
├── Services/                        # 业务逻辑层
│   └── WingetService.cs            # Winget 服务封装
│
├── ViewModels/                      # 视图模型层
│   ├── BrowseViewModel.cs          # 浏览页视图模型
│   ├── InstalledViewModel.cs       # 已安装页视图模型
│   └── UpdatesViewModel.cs         # 更新页视图模型
│
├── Views/                           # 视图层
│   ├── BrowsePage.xaml             # 浏览页面
│   ├── InstalledPage.xaml          # 已安装页面
│   ├── UpdatesPage.xaml            # 更新页面
│   └── SettingsPage.xaml           # 设置页面
│
├── Converters/                      # XAML 转换器
│   └── BoolToVisibilityConverter.cs # 布尔到可见性转换器
│
├── App.xaml                         # 应用程序资源
├── MainWindow.xaml                  # 主窗口
└── app.manifest                     # 应用清单（管理员权限）
```

## MVVM 架构详解

### Model（模型）

**Package.cs** - 定义软件包的数据结构
```csharp
public class Package
{
    public string Id { get; set; }              // 软件包 ID
    public string Name { get; set; }            // 软件名称
    public string Version { get; set; }         // 版本号
    public string Publisher { get; set; }       // 发布者
    public string Description { get; set; }     // 描述
    public bool IsInstalled { get; set; }       // 是否已安装
    public string InstalledVersion { get; set; } // 已安装版本
    public string AvailableVersion { get; set; } // 可用版本
}
```

### ViewModel（视图模型）

使用 `CommunityToolkit.Mvvm` 提供的特性：
- `ObservableObject` - 实现 INotifyPropertyChanged
- `ObservableProperty` - 自动生成属性通知
- `RelayCommand` - 命令绑定

**关键功能**：
- 数据绑定
- 命令处理
- 状态管理
- 异步操作

### View（视图）

使用 WinUI 3 控件：
- `NavigationView` - 导航视图
- `ListView` - 列表视图
- `AutoSuggestBox` - 搜索框
- `ProgressRing` - 加载指示器
- `InfoBar` - 信息栏

## Winget 集成

### WingetService 类

封装了所有 winget 命令行操作：

```csharp
// 搜索软件包
Task<List<Package>> SearchPackagesAsync(string query)

// 获取已安装软件
Task<List<Package>> GetInstalledPackagesAsync()

// 获取可更新软件
Task<List<Package>> GetUpgradeablePackagesAsync()

// 安装软件
Task<bool> InstallPackageAsync(string packageId, IProgress<string> progress)

// 卸载软件
Task<bool> UninstallPackageAsync(string packageId, IProgress<string> progress)

// 更新软件
Task<bool> UpgradePackageAsync(string packageId, IProgress<string> progress)
```

### 命令执行流程

1. 构建 winget 命令
2. 启动进程并重定向输出
3. 异步读取输出流
4. 解析输出文本
5. 返回结构化数据

## Fluent Design 元素

### 设计原则

1. **Light（光感）** - 使用阴影和高度营造层次感
2. **Depth（深度）** - 通过视差和分层创建空间感
3. **Motion（动效）** - 流畅的过渡动画
4. **Material（材质）** - Acrylic 亚克力效果
5. **Scale（缩放）** - 自适应不同屏幕尺寸

### UI 组件

- **Navigation View** - 汉堡菜单导航
- **Card Layout** - 卡片式列表项
- **Acrylic Background** - 半透明背景
- **Fluent Icons** - Segoe Fluent Icons 字体
- **Color System** - 系统主题色

## 数据绑定

### x:Bind 编译时绑定

```xaml
<!-- 单向绑定 -->
Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"

<!-- 双向绑定 -->
Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay}"

<!-- 命令绑定 -->
Command="{x:Bind ViewModel.RefreshCommand}"

<!-- 转换器绑定 -->
Visibility="{x:Bind IsInstalled, Converter={StaticResource BoolToVisibilityConverter}}"
```

## 异步操作

所有 winget 操作都是异步的：

```csharp
[RelayCommand]
private async Task RefreshAsync()
{
    IsLoading = true;
    var packages = await _wingetService.GetInstalledPackagesAsync();
    // 更新 UI
    IsLoading = false;
}
```

## 进度反馈

使用 `IProgress<string>` 接口报告操作进度：

```csharp
var progress = new Progress<string>(message => StatusMessage = message);
await _wingetService.InstallPackageAsync(packageId, progress);
```

## 权限管理

应用需要管理员权限以执行安装/卸载操作，在 `app.manifest` 中配置：

```xml
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```

## 构建和运行

### 开发环境要求

- Visual Studio 2022 (17.0+)
- .NET 8.0 SDK
- Windows App SDK 1.5+
- Windows 10 SDK (10.0.19041.0+)

### 构建步骤

1. 打开 `FluentSoftwareManager.sln`
2. 还原 NuGet 包
3. 选择 x64 平台
4. F5 运行或 Ctrl+Shift+B 构建

### 发布

```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

## 未来计划

### 短期目标
- [ ] 添加软件详情对话框
- [ ] 实现下载进度条
- [ ] 添加安装历史记录
- [ ] 支持取消操作

### 中期目标
- [ ] 多语言支持（中文、英文）
- [ ] 软件分类和标签
- [ ] 收藏夹功能
- [ ] 导出/导入软件列表

### 长期目标
- [ ] 自定义软件源
- [ ] 软件评分和评论
- [ ] 云同步配置
- [ ] 插件系统

## 性能优化

### 已实现
- 异步操作防止 UI 阻塞
- x:Bind 编译时绑定提升性能
- ListView 虚拟化减少内存占用

### 待优化
- 缓存搜索结果
- 分页加载大量数据
- 后台任务管理

## 调试技巧

### 查看 winget 输出
在 `WingetService.cs` 中添加调试输出：
```csharp
Debug.WriteLine($"Winget output: {output}");
```

### XAML 绑定调试
启用绑定跟踪：
```xaml
<Application ... xmlns:diagnostics="using:Microsoft.UI.Xaml.Diagnostics">
```

## 常见问题

### Q: 为什么需要管理员权限？
A: winget 的安装和卸载操作需要管理员权限来修改系统文件和注册表。

### Q: 如何处理 winget 不存在的情况？
A: 应用会捕获异常并在 UI 中显示错误信息，建议添加 winget 检测逻辑。

### Q: 支持哪些 Windows 版本？
A: 最低支持 Windows 10 1809 (Build 17763)，推荐使用 Windows 11。

## 贡献指南

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 代码规范

- 使用 C# 命名约定
- XAML 使用 4 空格缩进
- 添加 XML 文档注释
- 遵循 MVVM 模式

## 许可证

MIT License - 详见 LICENSE 文件

---

文档版本：1.0
最后更新：2024
