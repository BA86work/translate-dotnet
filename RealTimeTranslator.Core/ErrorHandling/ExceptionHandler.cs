using System;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RealTimeTranslator.Core.Exceptions;

public static class ExceptionHandler
{
    private static ILogger _logger;

    public static void Initialize(ILogger logger)
    {
        _logger = logger;
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        Application.Current.DispatcherUnhandledException += HandleDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
    }

    private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogAndNotify(exception, "Unhandled Exception");
    }

    private static void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogAndNotify(e.Exception, "Dispatcher Unhandled Exception");
        e.Handled = true;
    }

    private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LogAndNotify(e.Exception, "Unobserved Task Exception");
        e.SetObserved();
    }

    private static void LogAndNotify(Exception exception, string source)
    {
        _logger.LogError(exception, $"{source}: {exception.Message}");
        
        if (exception is TranslatorException translatorException)
        {
            ShowErrorNotification(translatorException);
        }
    }

    private static void ShowErrorNotification(TranslatorException exception)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var message = GetUserFriendlyMessage(exception);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }

    private static string GetUserFriendlyMessage(TranslatorException exception)
    {
        return exception.ErrorCode switch
        {
            TranslatorErrorCode.ApiKeyInvalid => "Invalid API key. Please check your settings.",
            TranslatorErrorCode.NetworkError => "Network error. Please check your internet connection.",
            TranslatorErrorCode.OcrFailed => "Error processing image text. Please try again.",
            TranslatorErrorCode.ScreenCaptureFailed => "Error capturing screen. Please try again.",
            TranslatorErrorCode.DatabaseError => "Database error. Please restart the application.",
            _ => "An unexpected error occurred. Please try again."
        };
    }
} 