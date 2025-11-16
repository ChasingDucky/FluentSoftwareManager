# 快速开始指南

## Windows 用户快速开始（5 分钟）

### 方式一：使用 PowerShell 脚本（最简单）

1. **克隆项目**
   ```powershell
   git clone https://github.com/yourusername/FluentSoftwareManager.git
   cd FluentSoftwareManager
   ```

2. **以管理员身份运行 PowerShell**
   - 右键点击开始菜单
   - 选择 "Windows PowerShell (管理员)" 或 "终端 (管理员)"

3. **运行应用**
   ```powershell
   .\run.ps1
   ```

就这么简单！应用会自动构建并启动。

### 方式二：使用 Visual Studio（推荐开发使用）

1. **安装 Visual Studio 2022**
   - 下载地址: https://visualstudio.microsoft.com/
   - 安装时选择 "Windows 应用程序开发" 工作负载

2. **以管理员身份运行 Visual Studio**
   - 右键点击 Visual Studio 图标
   - 选择 "以管理员身份运行"

3. **打开项目**
   - 文件 -> 打开 -> 项目/解决方案
   - 选择 `FluentSoftwareManager.sln`

4. **运行**
   - 选择 `Debug` 配置和 `x64` 平台
   - 按 `F5` 或点击 "开始" 按钮

### 方式三：使用 dotnet CLI

```powershell
# 1. 还原依赖
dotnet restore

# 2. 构建项目
dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

# 3. 运行应用（需要管理员权限）
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64
```

## 常用命令速查

### PowerShell 脚本

```powershell
# 构建项目
.\build.ps1                                    # Debug x64
.\build.ps1 -Configuration Release            # Release x64

# 运行项目
.\run.ps1                                     # Debug x64
.\run.ps1 -Configuration Release              # Release x64

# 发布项目
.\publish.ps1                                 # 框架依赖
.\publish.ps1 -SelfContained                  # 自包含

# 清理项目
.\clean.ps1
```

### dotnet CLI

```powershell
# 还原
dotnet restore

# 构建
dotnet build -c Debug -p:Platform=x64
dotnet build -c Release -p:Platform=x64

# 运行
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj -c Debug -p:Platform=x64

# 发布
dotnet publish -c Release -r win-x64 -p:Platform=x64 -o ./publish

# 清理
dotnet clean
```

## 首次运行检查清单

- [ ] 已安装 .NET 8.0 SDK (`dotnet --version`)
- [ ] 已安装 Windows App SDK
- [ ] 已安装 winget (`winget --version`)
- [ ] 以管理员权限运行
- [ ] Windows 10 1809+ 或 Windows 11

## 功能演示

### 1. 浏览软件
- 启动应用后默认在 "Browse" 页面
- 在搜索框输入软件名称，如 "VSCode"
- 点击 "Install" 按钮安装

### 2. 查看已安装软件
- 点击左侧导航栏的 "Installed"
- 查看所有已安装的软件列表
- 点击 "Uninstall" 卸载不需要的软件

### 3. 检查更新
- 点击左侧导航栏的 "Updates"
- 查看所有可更新的软件
- 点击 "Update" 更新单个软件
- 或点击 "Update All" 更新所有软件

### 4. 设置
- 点击左下角的设置图标
- 可以切换主题（浅色/深色/跟随系统）
- 查看应用信息

## 故障排除

### 问题 1: "找不到 dotnet 命令"

**解决方案**:
```powershell
# 安装 .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8

# 验证安装
dotnet --version
```

### 问题 2: "需要管理员权限"

**解决方案**:
- 右键点击 PowerShell 或命令提示符
- 选择 "以管理员身份运行"
- 重新执行命令

### 问题 3: "找不到 winget"

**解决方案**:
```powershell
# 从 Microsoft Store 安装 "应用安装程序"
# 或使用以下命令
winget --version  # 检查是否已安装
```

### 问题 4: 构建错误 - 找不到 SDK

**解决方案**:
```powershell
# 安装 Windows App SDK 工作负载
dotnet workload install microsoft-net-sdk-windowsdesktop

# 或通过 Visual Studio Installer 安装
```

### 问题 5: 应用启动后立即关闭

**可能原因**:
- 没有管理员权限
- winget 未安装或不可用
- .NET Runtime 版本不匹配

**解决方案**:
```powershell
# 1. 检查 winget
winget --version

# 2. 检查 .NET
dotnet --info

# 3. 以管理员身份运行
Start-Process powershell -Verb RunAs -ArgumentList "-File run.ps1"
```

## 开发建议

### VS Code 用户

1. 安装推荐扩展:
   - C# Dev Kit
   - .NET Extension Pack
   - XML Tools

2. 打开项目:
   ```powershell
   code .
   ```

3. 调试:
   - 按 `F5` 开始调试
   - 或使用命令面板: `Debug: Start Debugging`

### Visual Studio 用户

1. 设置启动项目:
   - 右键 FluentSoftwareManager 项目
   - 设为启动项目

2. 配置调试:
   - 项目属性 -> 调试
   - 确保选择 "不启动，但在代码启动时调试"

3. 快捷键:
   - `F5` - 开始调试
   - `Ctrl+F5` - 无调试运行
   - `Shift+F5` - 停止调试

## 性能提示

### 加快构建速度

```xml
<!-- 在 .csproj 中添加 -->
<PropertyGroup>
  <UseRidGraph>true</UseRidGraph>
  <ErrorOnDuplicatePublishOutputFiles>false</ErrorOnDuplicatePublishOutputFiles>
</PropertyGroup>
```

### 减小发布体积

```powershell
# 使用框架依赖发布
.\publish.ps1

# 而不是自包含发布
.\publish.ps1 -SelfContained
```

## 下一步

- 📖 阅读 [BUILD.md](BUILD.md) 了解详细构建选项
- 📖 阅读 [DOCS.md](DOCS.md) 了解项目架构
- 📖 阅读 [README.md](README.md) 了解功能特性
- 🐛 报告问题或建议到 GitHub Issues

## 更多帮助

如需更多帮助，请查看:
- 项目文档: [DOCS.md](DOCS.md)
- 构建指南: [BUILD.md](BUILD.md)
- GitHub Issues: https://github.com/yourusername/FluentSoftwareManager/issues

祝你使用愉快！🎉
