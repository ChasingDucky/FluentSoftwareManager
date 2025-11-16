# 构建和调试指南

本文档说明如何使用 dotnet 命令行工具构建、调试和打包 Fluent Software Manager。

## 前提条件

### 必需软件

1. **.NET 8.0 SDK**
   ```powershell
   # 检查是否已安装
   dotnet --version

   # 如果未安装，使用 winget 安装
   winget install Microsoft.DotNet.SDK.8
   ```

2. **Windows App SDK**
   - 通过 Visual Studio Installer 安装 "Windows 应用程序开发" 工作负载
   - 或访问：https://learn.microsoft.com/windows/apps/windows-app-sdk/

3. **Windows 10 SDK (19041 或更高)**
   ```powershell
   winget install Microsoft.WindowsSDK
   ```

### 可选工具

- **Visual Studio 2022** (用于 IDE 开发)
- **Visual Studio Code** (轻量级编辑器)

## 使用 PowerShell 脚本（推荐）

我们提供了几个 PowerShell 脚本来简化常见任务：

### 1. 构建项目

```powershell
# 使用默认配置 (Debug, x64)
.\build.ps1

# 指定配置
.\build.ps1 -Configuration Release -Platform x64

# 可选平台: x64, x86, ARM64
```

### 2. 运行项目

```powershell
# 以管理员身份运行（推荐）
.\run.ps1

# 指定配置运行
.\run.ps1 -Configuration Debug -Platform x64
```

### 3. 发布项目

```powershell
# 框架依赖发布（需要用户安装 .NET Runtime）
.\publish.ps1 -Platform x64

# 自包含发布（包含 .NET Runtime，文件较大）
.\publish.ps1 -Platform x64 -SelfContained

# 指定输出路径
.\publish.ps1 -Platform x64 -OutputPath "D:\MyPublish"
```

### 4. 清理项目

```powershell
# 删除所有构建产物
.\clean.ps1
```

## 使用 dotnet 命令行

如果你更喜欢直接使用 dotnet 命令：

### 还原依赖

```powershell
dotnet restore FluentSoftwareManager/FluentSoftwareManager.csproj
```

### 构建项目

```powershell
# Debug 构建
dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

# Release 构建
dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj -c Release -p:Platform=x64
```

### 运行项目

```powershell
# 直接运行（会先构建）
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

# 运行已构建的程序
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64 --no-build
```

**注意**: 应用程序需要管理员权限，请确保以管理员身份运行 PowerShell 或命令提示符。

### 发布项目

```powershell
# 框架依赖发布
dotnet publish FluentSoftwareManager/FluentSoftwareManager.csproj `
    -c Release `
    -r win-x64 `
    -p:Platform=x64 `
    -o ./publish `
    --self-contained false

# 自包含发布
dotnet publish FluentSoftwareManager/FluentSoftwareManager.csproj `
    -c Release `
    -r win-x64 `
    -p:Platform=x64 `
    -o ./publish `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true
```

### 清理构建

```powershell
# 清理构建产物
dotnet clean FluentSoftwareManager/FluentSoftwareManager.csproj

# 完全清理
Remove-Item -Path FluentSoftwareManager/bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path FluentSoftwareManager/obj -Recurse -Force -ErrorAction SilentlyContinue
```

## 调试

### 使用 Visual Studio

1. 以**管理员身份**运行 Visual Studio 2022
2. 打开 `FluentSoftwareManager.sln`
3. 选择 `Debug` 配置和 `x64` 平台
4. 按 `F5` 开始调试

### 使用 Visual Studio Code

1. 安装 C# Dev Kit 扩展
2. 以**管理员身份**运行 VS Code
3. 打开项目文件夹
4. 按 `F5` 开始调试

### 使用 dotnet CLI

虽然 dotnet CLI 不直接支持图形化调试，但你可以：

1. 添加 `Console.WriteLine()` 或使用 `Debug.WriteLine()`
2. 查看输出窗口的调试信息
3. 使用日志库（如 Serilog, NLog）

```csharp
// 在代码中添加调试输出
using System.Diagnostics;

Debug.WriteLine($"Package count: {packages.Count}");
```

## 常见问题

### Q1: 构建失败 - 找不到 SDK

**解决方案**:
```powershell
# 安装 Windows App SDK
dotnet workload install microsoft-net-sdk-windowsdesktop
```

### Q2: 需要管理员权限

**解决方案**:
```powershell
# 以管理员身份运行 PowerShell
Start-Process powershell -Verb RunAs
```

### Q3: 找不到 winget

**解决方案**:
应用运行时需要 winget。从 Microsoft Store 安装 "应用安装程序" 或访问：
https://github.com/microsoft/winget-cli/releases

### Q4: 发布后文件太大

**解决方案**:
```powershell
# 使用框架依赖发布而不是自包含
.\publish.ps1 -Platform x64
# 而不是
.\publish.ps1 -Platform x64 -SelfContained
```

### Q5: 运行时崩溃

**解决方案**:
1. 检查是否以管理员权限运行
2. 检查 winget 是否可用：`winget --version`
3. 查看应用事件日志

## 发布选项对比

| 选项 | 框架依赖 | 自包含 |
|------|----------|--------|
| 文件大小 | ~5-10 MB | ~80-120 MB |
| 需要 .NET Runtime | 是 | 否 |
| 启动速度 | 快 | 较快 |
| 适用场景 | 开发者/技术用户 | 普通用户 |

## 性能优化

### ReadyToRun (R2R)

启用 ReadyToRun 可以提升启动性能：

```xml
<!-- 在 .csproj 中添加 -->
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

### 单文件发布

减少文件数量，便于分发：

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

### 裁剪未使用代码

减小发布体积（可能导致反射问题）：

```xml
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>link</TrimMode>
</PropertyGroup>
```

## 持续集成 (CI/CD)

### GitHub Actions 示例

```yaml
name: Build and Publish

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore -p:Platform=x64

    - name: Publish
      run: dotnet publish -c Release -r win-x64 -p:Platform=x64 --self-contained true

    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: FluentSoftwareManager
        path: publish/
```

## 测试

虽然当前项目没有单元测试，但你可以添加：

```powershell
# 创建测试项目
dotnet new mstest -n FluentSoftwareManager.Tests

# 添加项目引用
cd FluentSoftwareManager.Tests
dotnet add reference ../FluentSoftwareManager/FluentSoftwareManager.csproj

# 运行测试
dotnet test
```

## 打包为 MSIX

WinUI 3 应用可以打包为 MSIX 格式：

```powershell
# 使用 Visual Studio
# 1. 右键项目 -> 发布 -> 创建应用包
# 2. 选择 Microsoft Store 或旁加载
# 3. 按向导完成

# 或使用命令行
msbuild FluentSoftwareManager/FluentSoftwareManager.csproj `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /p:UapAppxPackageBuildMode=SideloadOnly `
    /p:AppxBundle=Always `
    /p:AppxPackageDir="./AppPackages/"
```

## 更多资源

- [.NET CLI 文档](https://learn.microsoft.com/dotnet/core/tools/)
- [WinUI 3 文档](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK 文档](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- [发布应用指南](https://learn.microsoft.com/dotnet/core/deploying/)

---

如有问题，请提交 Issue 或查看项目文档。
