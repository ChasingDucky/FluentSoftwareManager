using Serilog;
using Serilog.Events;
using System.IO;

namespace FluentSoftwareManager.Services;

public static class LogService
{
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized) return;

        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FluentSoftwareManager",
            "Logs"
        );

        Directory.CreateDirectory(logFolder);

        var logFile = Path.Combine(logFolder, "app.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FluentSoftwareManager")
            .Enrich.WithProperty("Version", GetAppVersion())
            .WriteTo.File(
                logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.Debug()
            .CreateLogger();

        _isInitialized = true;

        Log.Information("=== Application Started ===");
        Log.Information("Version: {Version}", GetAppVersion());
        Log.Information("OS: {OS}", Environment.OSVersion);
        Log.Information(".NET: {DotNetVersion}", Environment.Version);
        Log.Information("Log Path: {LogPath}", logFile);
    }

    public static void Shutdown()
    {
        Log.Information("=== Application Shutdown ===");
        Log.CloseAndFlush();
    }

    public static string GetLogFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FluentSoftwareManager",
            "Logs"
        );
    }

    public static string GetAppVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0.0";
    }

    public static void LogException(Exception ex, string context = "")
    {
        Log.Error(ex, "Exception in {Context}", context);
    }
}
