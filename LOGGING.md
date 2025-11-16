# 日志和错误处理

本文档说明 Fluent Software Manager 的日志系统和错误处理机制。

## 日志系统

### 日志框架

应用使用 **Serilog** 作为日志框架，支持：
- 结构化日志
- 多个输出目标（文件、调试控制台）
- 日志级别过滤
- 自动日志轮转

### 日志位置

日志文件存储在：
```
%LOCALAPPDATA%\FluentSoftwareManager\Logs\
```

完整路径示例：
```
C:\Users\<YourName>\AppData\Local\FluentSoftwareManager\Logs\app20241116.log
```

### 日志级别

| 级别 | 用途 | 示例 |
|------|------|------|
| Verbose | 详细调试信息 | Winget 输出的每一行 |
| Debug | 调试信息 | 方法调用、参数 |
| Information | 一般信息 | 应用启动、操作完成 |
| Warning | 警告信息 | Winget 错误输出 |
| Error | 错误信息 | 操作失败、异常 |
| Fatal | 致命错误 | 应用崩溃 |

### 日志配置

在 `LogService.cs` 中配置：

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.File(logFile,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .WriteTo.Debug()
    .CreateLogger();
```

配置说明：
- 最小日志级别：Debug
- 日志轮转：每天一个新文件
- 保留天数：7 天
- 输出目标：文件 + 调试控制台

### 日志格式

```
2024-11-16 10:30:45.123 +08:00 [INF] Application Started
2024-11-16 10:30:45.456 +08:00 [INF] Version: 1.0.0.0
2024-11-16 10:30:46.789 +08:00 [DBG] SearchPackagesAsync called with query: "VSCode"
2024-11-16 10:30:47.012 +08:00 [INF] Found 5 packages
2024-11-16 10:30:50.345 [ERR] Error installing package: VSCode
System.Exception: Winget command failed
   at FluentSoftwareManager.Services.WingetService.ExecuteWingetCommandAsync...
```

## 错误处理

### 全局异常捕获

应用在三个级别捕获未处理的异常：

#### 1. UI 线程异常

```csharp
this.UnhandledException += OnUnhandledException;
```

处理：
- 记录到日志
- 显示错误对话框
- 标记为已处理（e.Handled = true）

#### 2. AppDomain 异常

```csharp
AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
```

处理：
- 记录到日志
- 生成崩溃报告
- 如果是致命错误，保存诊断信息后退出

#### 3. 异步任务异常

```csharp
TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
```

处理：
- 记录到日志
- 标记为已观察（e.SetObserved()）

### 错误对话框

使用 `ErrorDialog` 服务显示友好的错误信息：

```csharp
// 显示错误
await ErrorDialog.ShowErrorAsync(
    "Operation Failed",
    "Failed to install package",
    exception
);

// 显示信息
await ErrorDialog.ShowInfoAsync(
    "Success",
    "Package installed successfully"
);

// 显示确认对话框
bool confirmed = await ErrorDialog.ShowConfirmAsync(
    "Confirm Uninstall",
    "Are you sure you want to uninstall this package?"
);
```

### WingetService 错误处理

所有 winget 操作都包含完整的错误处理：

```csharp
public async Task<bool> InstallPackageAsync(string packageId, IProgress<string>? progress = null)
{
    Log.Information("Installing package: {PackageId}", packageId);
    try
    {
        // 执行安装
        var output = await ExecuteWingetCommandAsync(...);
        Log.Information("Successfully installed package: {PackageId}", packageId);
        return true;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error installing package: {PackageId}", packageId);
        progress?.Report($"Installation failed: {ex.Message}");
        return false;
    }
}
```

## 诊断工具

### DiagnosticsService

提供系统诊断功能：

```csharp
// 生成诊断报告
string report = await DiagnosticsService.GenerateDiagnosticReportAsync();

// 保存诊断报告
await DiagnosticsService.SaveDiagnosticReportAsync("report.txt");
```

诊断报告包含：
- 应用信息（版本、日志路径）
- 系统信息（OS、.NET 版本）
- .NET Runtime 信息
- Winget 版本和状态
- Windows App SDK 状态
- 内存使用情况
- 最近的日志条目

### 崩溃报告

当应用崩溃时，自动生成崩溃报告：

位置：
```
%LOCALAPPDATA%\FluentSoftwareManager\Logs\crash-<timestamp>.txt
```

示例：
```
C:\Users\<YourName>\AppData\Local\FluentSoftwareManager\Logs\crash-20241116-103045.txt
```

## 查看日志

### 方式 1：直接打开日志文件

```powershell
# 打开日志文件夹
explorer %LOCALAPPDATA%\FluentSoftwareManager\Logs

# 查看今天的日志
notepad %LOCALAPPDATA%\FluentSoftwareManager\Logs\app*.log
```

### 方式 2：使用 PowerShell

```powershell
# 查看最新的 20 行日志
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" -Tail 20

# 实时监控日志（类似 tail -f）
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" -Wait -Tail 10

# 搜索错误
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" | Select-String "ERR"

# 查看特定时间的日志
Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log" | Select-String "2024-11-16 10:"
```

### 方式 3：使用日志查看工具

推荐工具：
- **Notepad++** - 高亮显示、实时更新
- **BareTail** - 专业的日志查看工具
- **Serilog Analyzer** - 结构化日志分析

## 前置条件检查

应用启动时会检查必要条件：

```csharp
private bool CheckPrerequisites()
{
    try
    {
        // 检查 winget 是否可用
        var wingetCheck = Process.Start(...);

        if (wingetCheck == null)
        {
            Log.Error("Winget executable not found");
            ShowStartupError(...);
            return false;
        }

        return true;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error checking prerequisites");
        return false;
    }
}
```

检查项：
- ✅ Winget 是否安装
- ✅ Winget 是否可执行
- ✅ 系统权限是否足够

## 日志使用示例

### 在 ViewModel 中记录日志

```csharp
using Serilog;

public class BrowseViewModel : ObservableObject
{
    [RelayCommand]
    private async Task SearchAsync()
    {
        Log.Debug("Search command executed");

        try
        {
            IsLoading = true;
            Log.Information("Searching for packages: {Query}", SearchQuery);

            var packages = await _wingetService.SearchPackagesAsync(SearchQuery);

            Log.Information("Found {Count} packages", packages.Count);
            Packages.Clear();
            foreach (var package in packages)
            {
                Packages.Add(package);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during package search");
            await ErrorDialog.ShowErrorAsync("Search Failed", "Failed to search packages", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### 记录性能信息

```csharp
using System.Diagnostics;
using Serilog;

var stopwatch = Stopwatch.StartNew();

var packages = await _wingetService.SearchPackagesAsync(query);

stopwatch.Stop();
Log.Information("Search completed in {ElapsedMs}ms, found {Count} packages",
    stopwatch.ElapsedMilliseconds,
    packages.Count);
```

## 故障排除

### 问题 1：日志文件过大

**原因**：调试级别日志会产生大量数据

**解决方案**：
```csharp
// 在 LogService.cs 中修改最小级别
.MinimumLevel.Information()  // 从 Debug 改为 Information
```

### 问题 2：找不到日志文件

**解决方案**：
```powershell
# 检查日志路径
$logPath = "$env:LOCALAPPDATA\FluentSoftwareManager\Logs"
Write-Host "Log path: $logPath"
Test-Path $logPath
```

### 问题 3：日志没有写入

**可能原因**：
- 权限问题
- 磁盘空间不足
- 应用崩溃在日志初始化之前

**解决方案**：
```csharp
// 添加 try-catch 到日志初始化
try
{
    LogService.Initialize();
}
catch (Exception ex)
{
    // Fallback to event log or console
    Console.WriteLine($"Failed to initialize logging: {ex}");
}
```

### 问题 4：无法显示错误对话框

**原因**：XamlRoot 未设置

**解决方案**：
确保 App.MainWindow 已正确设置：
```csharp
protected override void OnLaunched(...)
{
    m_window = new MainWindow();
    App.MainWindow = m_window;  // 重要！
    m_window.Activate();
}
```

## 最佳实践

### 1. 记录重要操作

```csharp
// ✅ 好的做法
Log.Information("Installing package: {PackageId}", packageId);
await _wingetService.InstallPackageAsync(packageId);
Log.Information("Package installed successfully: {PackageId}", packageId);

// ❌ 不好的做法
await _wingetService.InstallPackageAsync(packageId);  // 没有日志
```

### 2. 记录异常时包含上下文

```csharp
// ✅ 好的做法
Log.Error(ex, "Failed to install package {PackageId} for user {User}",
    packageId, userName);

// ❌ 不好的做法
Log.Error(ex, "Installation failed");  // 缺少上下文
```

### 3. 使用结构化日志

```csharp
// ✅ 好的做法
Log.Information("Search completed: Query={Query}, Results={Count}, Duration={Ms}ms",
    query, results.Count, duration);

// ❌ 不好的做法
Log.Information($"Search completed: {query}, found {results.Count} in {duration}ms");
```

### 4. 适当的日志级别

```csharp
Log.Verbose("Processing item {Index} of {Total}", i, total);     // 过于详细
Log.Debug("Executing winget command: {Command}", command);       // 调试信息
Log.Information("Package installed: {PackageId}", packageId);    // 正常操作
Log.Warning("Package version mismatch: {PackageId}", packageId); // 警告
Log.Error(ex, "Installation failed: {PackageId}", packageId);    // 错误
Log.Fatal(ex, "Application crash");                              // 致命错误
```

### 5. 及时释放资源

```csharp
// 应用退出时
protected override void OnClosed(...)
{
    Log.Information("Application closing");
    LogService.Shutdown();  // 刷新所有日志
}
```

## 监控和分析

### 实时监控脚本

创建 `monitor-logs.ps1`：

```powershell
$logPath = "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log"
Write-Host "Monitoring logs at: $logPath" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

Get-Content $logPath -Wait -Tail 0 | ForEach-Object {
    if ($_ -match "\[ERR\]") {
        Write-Host $_ -ForegroundColor Red
    }
    elseif ($_ -match "\[WRN\]") {
        Write-Host $_ -ForegroundColor Yellow
    }
    elseif ($_ -match "\[INF\]") {
        Write-Host $_ -ForegroundColor Green
    }
    else {
        Write-Host $_
    }
}
```

### 错误统计

```powershell
# 统计各级别日志数量
$log = Get-Content "$env:LOCALAPPDATA\FluentSoftwareManager\Logs\app*.log"
$log | Select-String "\[ERR\]" | Measure-Object | Select-Object Count
$log | Select-String "\[WRN\]" | Measure-Object | Select-Object Count
$log | Select-String "\[INF\]" | Measure-Object | Select-Object Count
```

## 更多资源

- [Serilog 官方文档](https://serilog.net/)
- [日志最佳实践](https://docs.microsoft.com/aspnet/core/fundamentals/logging/)
- 项目文档：[DOCS.md](DOCS.md)

---

如有问题，请查看日志文件或提交 Issue。
