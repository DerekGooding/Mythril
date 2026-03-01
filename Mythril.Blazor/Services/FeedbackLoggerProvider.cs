using Microsoft.Extensions.Logging;

namespace Mythril.Blazor.Services;

public class FeedbackLoggerProvider(FeedbackService feedbackService, AuthService authService) : ILoggerProvider
{
    private readonly FeedbackService _feedbackService = feedbackService;
    private readonly AuthService _authService = authService;

    public ILogger CreateLogger(string categoryName) => new FeedbackLogger(_feedbackService, _authService);

    public void Dispose() { }

    private class FeedbackLogger(FeedbackService feedbackService, AuthService authService) : ILogger
    {
        private readonly FeedbackService _feedbackService = feedbackService;
        private readonly AuthService _authService = authService;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error && _authService.IsAuthenticated;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            Console.WriteLine($"[FeedbackLogger] Capturing Error: {message}");
            // Fire and forget to avoid blocking UI thread
            _ = _feedbackService.CaptureError(message, exception?.StackTrace);
        }
    }
}
