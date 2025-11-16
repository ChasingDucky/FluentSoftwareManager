using Microsoft.UI.Xaml;
using FluentSoftwareManager.Services;
using Serilog;
using System;

namespace FluentSoftwareManager;

public partial class App : Application
{
    private Window? m_window;
    public static Window? MainWindow { get; private set; }

    public App()
    {
        // Initialize logging first
        LogService.Initialize();
        Log.Information("Application constructor called");

        this.InitializeComponent();

        // Setup global exception handlers
        this.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        Log.Information("Application initialized successfully");
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            Log.Information("OnLaunched called");

            // Check prerequisites
            if (!CheckPrerequisites())
            {
                Log.Error("Prerequisites check failed");
                return;
            }

            m_window = new MainWindow();
            MainWindow = m_window;

            m_window.Closed += (s, e) =>
            {
                Log.Information("Main window closed");
                LogService.Shutdown();
            };

            m_window.Activate();
            Log.Information("Main window activated");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application launch");
            LogService.Shutdown();
            throw;
        }
    }

    private bool CheckPrerequisites()
    {
        try
        {
            // Check if winget is available
            var wingetCheck = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "winget",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (wingetCheck == null)
            {
                Log.Error("Winget executable not found");
                ShowStartupError(
                    "Windows Package Manager (winget) is not installed or not accessible.",
                    "Please install winget from Microsoft Store (App Installer) or visit:\nhttps://github.com/microsoft/winget-cli/releases"
                );
                return false;
            }

            wingetCheck.WaitForExit(5000);
            Log.Information("Winget is available");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking prerequisites");
            ShowStartupError(
                "Failed to verify system prerequisites.",
                $"Error: {ex.Message}\n\nPlease ensure Windows Package Manager (winget) is installed."
            );
            return false;
        }
    }

    private void ShowStartupError(string message, string details)
    {
        var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            Title = "Startup Error",
            Content = $"{message}\n\n{details}",
            CloseButtonText = "Exit",
            DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close
        };

        // Create a temporary window to show the dialog
        var tempWindow = new Window
        {
            Title = "Error"
        };
        tempWindow.Activate();

        if (tempWindow.Content is Microsoft.UI.Xaml.FrameworkElement element)
        {
            dialog.XamlRoot = element.XamlRoot;
            _ = dialog.ShowAsync();
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled exception in UI thread");
        e.Handled = true;

        try
        {
            _ = ErrorDialog.ShowErrorAsync(
                "Unexpected Error",
                "An unexpected error occurred. The application may be unstable.",
                e.Exception
            );
        }
        catch
        {
            // Last resort - just log it
            Log.Fatal("Failed to show error dialog for unhandled exception");
        }
    }

    private void OnDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled domain exception (IsTerminating: {IsTerminating})", e.IsTerminating);

            if (e.IsTerminating)
            {
                // Save diagnostic report
                try
                {
                    var reportPath = System.IO.Path.Combine(
                        LogService.GetLogFolder(),
                        $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.txt"
                    );
                    DiagnosticsService.SaveDiagnosticReportAsync(reportPath).Wait();
                    Log.Information("Crash report saved to {ReportPath}", reportPath);
                }
                catch
                {
                    // Ignore
                }

                LogService.Shutdown();
            }
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved(); // Prevent the process from crashing
    }
}
