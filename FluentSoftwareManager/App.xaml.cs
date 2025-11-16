using Microsoft.UI.Xaml;
using FluentSoftwareManager.Services;
using Serilog;
using System;
using System.Diagnostics;

namespace FluentSoftwareManager;

public partial class App : Application
{
    private Window? m_window;
    public static Window? MainWindow { get; private set; }

    public App()
    {
        try
        {
            // Initialize logging - wrapped in try-catch
            LogService.Initialize();
            Log.Information("Application constructor called");

            this.InitializeComponent();

            // Setup global exception handlers
            this.UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Log.Information("Application initialized successfully");
        }
        catch (Exception ex)
        {
            // Last resort error handling
            Debug.WriteLine($"FATAL: Application constructor failed: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");

            // Try to show error using Windows MessageBox
            try
            {
                var title = "Fluent Software Manager - Startup Error";
                var message = $"Failed to initialize application:\n\n{ex.Message}\n\nPlease check event logs for details.";
                ShowNativeErrorDialog(title, message);
            }
            catch
            {
                // Silently fail if we can't even show error dialog
            }

            throw;
        }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            Log.Information("OnLaunched called");

            // Check prerequisites first (without creating UI)
            string? prerequisiteError = CheckPrerequisites();
            if (prerequisiteError != null)
            {
                Log.Error("Prerequisites check failed: {Error}", prerequisiteError);
                ShowNativeErrorDialog("Prerequisites Missing", prerequisiteError);
                Log.Information("Exiting due to missing prerequisites");
                LogService.Shutdown();
                Environment.Exit(1);
                return;
            }

            // Create and show main window
            m_window = new MainWindow();
            MainWindow = m_window;

            m_window.Closed += (s, e) =>
            {
                Log.Information("Main window closed");
                LogService.Shutdown();
            };

            m_window.Activate();
            Log.Information("Main window activated successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application launch");

            try
            {
                var message = $"Failed to start application:\n\n{ex.Message}\n\nCheck logs at:\n{LogService.GetLogFolder()}";
                ShowNativeErrorDialog("Application Error", message);
            }
            catch
            {
                // Ignore
            }

            LogService.Shutdown();
            throw;
        }
    }

    private string? CheckPrerequisites()
    {
        try
        {
            // Check if winget is available
            Log.Information("Checking for winget...");

            var wingetCheck = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "winget",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (wingetCheck == null)
            {
                Log.Error("Winget executable not found");
                return "Windows Package Manager (winget) is not installed or not accessible.\n\n" +
                       "Please install winget from Microsoft Store (App Installer) or visit:\n" +
                       "https://github.com/microsoft/winget-cli/releases";
            }

            wingetCheck.WaitForExit(5000);

            if (wingetCheck.ExitCode != 0)
            {
                Log.Error("Winget check failed with exit code: {ExitCode}", wingetCheck.ExitCode);
                return "Windows Package Manager (winget) is not functioning properly.\n\n" +
                       "Please reinstall winget from Microsoft Store.";
            }

            Log.Information("Winget is available and working");
            return null; // All checks passed
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking prerequisites");
            return $"Failed to verify system prerequisites:\n\n{ex.Message}\n\n" +
                   "Please ensure Windows Package Manager (winget) is installed.";
        }
    }

    private void ShowNativeErrorDialog(string title, string message)
    {
        try
        {
            // Use native Windows MessageBox via P/Invoke
            var result = System.Windows.Forms.MessageBox.Show(
                message,
                title,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error
            );
        }
        catch
        {
            // Fallback to Debug output
            Debug.WriteLine($"ERROR: {title}\n{message}");
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        try
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
            catch (Exception dialogEx)
            {
                Log.Error(dialogEx, "Failed to show error dialog");

                // Fallback to native dialog
                try
                {
                    ShowNativeErrorDialog(
                        "Unexpected Error",
                        $"An error occurred:\n\n{e.Exception.Message}\n\nCheck logs for details."
                    );
                }
                catch
                {
                    // Give up
                }
            }
        }
        catch (Exception logEx)
        {
            Debug.WriteLine($"Failed to handle exception: {logEx.Message}");
        }
    }

    private void OnDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        try
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
                    catch (Exception reportEx)
                    {
                        Debug.WriteLine($"Failed to save crash report: {reportEx.Message}");
                    }

                    LogService.Shutdown();
                }
            }
        }
        catch (Exception handlerEx)
        {
            Debug.WriteLine($"Error in domain exception handler: {handlerEx.Message}");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved(); // Prevent the process from crashing
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in task exception handler: {ex.Message}");
        }
    }
}
