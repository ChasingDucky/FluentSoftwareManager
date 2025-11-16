using Serilog;
using Serilog.Events;
using System.IO;
using System.Diagnostics;

namespace FluentSoftwareManager.Services;

public static class LogService
{
    private static bool _isInitialized = false;
    private static bool _initializationFailed = false;

    public static void Initialize()
    {
        if (_isInitialized || _initializationFailed) return;

        try
        {
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
        catch (Exception ex)
        {
            // Fallback to Debug output if file logging fails
            _initializationFailed = true;
            Debug.WriteLine($"Failed to initialize logging: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // Try to at least setup debug logging
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Debug()
                    .CreateLogger();

                Log.Warning("File logging failed, using debug output only: {Error}", ex.Message);
                _isInitialized = true;
            }
            catch
            {
                // Complete failure - give up on logging
                Debug.WriteLine("Complete logging initialization failure");
            }
        }
    }

    public static void Shutdown()
    {
        try
        {
            if (_isInitialized)
            {
                Log.Information("=== Application Shutdown ===");
                Log.CloseAndFlush();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during log shutdown: {ex.Message}");
        }
    }

    public static string GetLogFolder()
    {
        try
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FluentSoftwareManager",
                "Logs"
            );
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetAppVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }
        catch
        {
            return "1.0.0.0";
        }
    }

    public static void LogException(Exception ex, string context = "")
    {
        try
        {
            if (_isInitialized)
            {
                Log.Error(ex, "Exception in {Context}", context);
            }
            else
            {
                Debug.WriteLine($"Exception in {context}: {ex.Message}");
            }
        }
        catch
        {
            Debug.WriteLine($"Failed to log exception: {ex.Message}");
        }
    }
}
