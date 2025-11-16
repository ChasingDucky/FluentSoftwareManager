# 命令速查表

快速参考所有可用的构建、运行和发布命令。

## PowerShell 脚本（推荐）

### 构建

```powershell
# 默认 Debug x64 构建
.\build.ps1

# Release 构建
.\build.ps1 -Configuration Release

# 指定平台
.\build.ps1 -Configuration Debug -Platform x64
.\build.ps1 -Configuration Release -Platform ARM64
```

### 运行

```powershell
# 默认 Debug x64 运行（会先构建）
.\run.ps1

# Release 运行
.\run.ps1 -Configuration Release

# 指定平台
.\run.ps1 -Configuration Debug -Platform x64
```

### 发布

```powershell
# 框架依赖发布（文件较小，需要 .NET Runtime）
.\publish.ps1 -Platform x64

# 自包含发布（文件较大，包含 .NET Runtime）
.\publish.ps1 -Platform x64 -SelfContained

# 指定输出路径
.\publish.ps1 -Platform x64 -OutputPath "D:\MyApp"
```

### 清理

```powershell
# 删除所有构建产物
.\clean.ps1
```

## 批处理脚本（CMD）

### 构建

```batch
REM 默认 Debug x64 构建
build.bat

REM 指定配置和平台
build.bat Release x64
build.bat Debug ARM64
```

### 运行

```batch
REM 默认 Debug x64 运行
run.bat

REM 指定配置和平台
run.bat Release x64
```

### 清理

```batch
REM 删除所有构建产物
clean.bat
```

## dotnet CLI 命令

### 还原依赖

```powershell
dotnet restore
dotnet restore FluentSoftwareManager/FluentSoftwareManager.csproj
```

### 构建

```powershell
# Debug 构建
dotnet build -c Debug -p:Platform=x64

# Release 构建
dotnet build -c Release -p:Platform=x64

# 完整命令
dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj -c Release -p:Platform=x64 --no-restore
```

### 运行

```powershell
# 运行（会先构建）
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

# 运行已构建的程序
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64 --no-build
```

### 发布

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

# 优化发布（单文件 + ReadyToRun）
dotnet publish FluentSoftwareManager/FluentSoftwareManager.csproj `
    -c Release `
    -r win-x64 `
    -p:Platform=x64 `
    -o ./publish `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true
```

### 清理

```powershell
# 清理构建产物
dotnet clean

# 完整命令
dotnet clean FluentSoftwareManager/FluentSoftwareManager.csproj
```

## Visual Studio 2022

### 快捷键

- `Ctrl+Shift+B` - 构建解决方案
- `F5` - 开始调试
- `Ctrl+F5` - 无调试运行
- `Shift+F5` - 停止调试
- `F9` - 设置/移除断点
- `F10` - 逐过程
- `F11` - 逐语句

### 构建配置

1. 选择配置：顶部工具栏 `Debug` 或 `Release`
2. 选择平台：顶部工具栏 `x64`、`x86` 或 `ARM64`
3. 右键项目 -> 属性 -> 配置属性

### 发布

1. 右键项目 -> 发布
2. 选择目标：
   - **文件夹** - 本地发布
   - **Microsoft Store** - 应用商店发布
   - **旁加载** - MSIX 包
3. 配置设置 -> 发布

## Visual Studio Code

### 任务（Tasks）

```
Ctrl+Shift+P -> Tasks: Run Task
```

可用任务：
- `build` - 构建项目 (Debug x64)
- `build-release` - 构建项目 (Release x64)
- `clean` - 清理构建产物
- `restore` - 还原 NuGet 包
- `publish` - 发布项目
- `run` - 运行项目

### 调试（Debug）

```
F5 或 Ctrl+F5
```

可用配置：
- `Launch (Debug)` - Debug 模式启动
- `Launch (Release)` - Release 模式启动
- `Attach to Process` - 附加到进程

## MSBuild 命令

```powershell
# 构建
msbuild FluentSoftwareManager.sln /p:Configuration=Release /p:Platform=x64

# 清理
msbuild FluentSoftwareManager.sln /t:Clean /p:Configuration=Release /p:Platform=x64

# 重新构建
msbuild FluentSoftwareManager.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64

# 发布
msbuild FluentSoftwareManager/FluentSoftwareManager.csproj `
    /t:Publish `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /p:RuntimeIdentifier=win-x64 `
    /p:PublishDir=./publish/
```

## NuGet 包管理

```powershell
# 还原所有包
dotnet restore

# 列出已安装的包
dotnet list package

# 更新包
dotnet add package <PackageName> --version <Version>

# 移除包
dotnet remove package <PackageName>
```

## 常见组合命令

### 完整构建流程

```powershell
# 1. 清理
.\clean.ps1

# 2. 还原
dotnet restore

# 3. 构建
.\build.ps1 -Configuration Release

# 4. 运行
.\run.ps1 -Configuration Release
```

### 快速测试

```powershell
# 构建并运行（一步到位）
.\run.ps1
```

### 准备发布

```powershell
# 1. 清理
.\clean.ps1

# 2. Release 构建
.\build.ps1 -Configuration Release

# 3. 发布
.\publish.ps1 -Platform x64 -SelfContained
```

## 运行时参数

虽然当前应用不支持命令行参数，但如果需要添加：

```csharp
// 在 App.xaml.cs 中
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    string[] cmdArgs = Environment.GetCommandLineArgs();
    // 处理参数
}
```

然后可以这样运行：

```powershell
.\FluentSoftwareManager.exe --silent --package "VSCode"
```

## 环境变量

```powershell
# 设置构建配置
$env:Configuration = "Release"

# 设置平台
$env:Platform = "x64"

# 设置详细日志
$env:MSBuildVerbosity = "detailed"
```

## 调试技巧

### 启用详细日志

```powershell
# dotnet 详细日志
dotnet build -v detailed

# MSBuild 详细日志
msbuild /v:detailed
```

### 查看生成的文件

```powershell
# 构建后查看输出
explorer FluentSoftwareManager\bin\Debug\net8.0-windows10.0.19041.0\x64\

# 发布后查看输出
explorer publish\
```

### 性能分析

```powershell
# 使用 dotnet trace
dotnet trace collect --process-id <PID>

# 使用 Visual Studio Profiler
# 调试 -> 性能探查器
```

## 持续集成

### GitHub Actions

见项目根目录的 `.github/workflows/build.yml`

### Azure DevOps

```yaml
trigger:
- main

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: dotnet restore
  displayName: 'Restore'

- script: dotnet build --configuration Release -p:Platform=x64
  displayName: 'Build'

- script: dotnet publish -c Release -r win-x64 -p:Platform=x64 -o $(Build.ArtifactStagingDirectory)
  displayName: 'Publish'

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)'
    artifactName: 'FluentSoftwareManager'
```

## 故障排除命令

```powershell
# 检查 .NET 安装
dotnet --info

# 检查 SDK 版本
dotnet --list-sdks

# 检查 Runtime 版本
dotnet --list-runtimes

# 验证 winget
winget --version

# 检查项目配置
dotnet msbuild FluentSoftwareManager/FluentSoftwareManager.csproj /t:ShowProperties
```

## 更多信息

- 详细构建指南: [BUILD.md](BUILD.md)
- 快速入门: [QUICK_START.md](QUICK_START.md)
- 项目文档: [DOCS.md](DOCS.md)
- 主页: [README.md](README.md)

---

💡 提示：所有脚本都支持 `-?` 或 `--help` 参数查看帮助（如果已实现）
