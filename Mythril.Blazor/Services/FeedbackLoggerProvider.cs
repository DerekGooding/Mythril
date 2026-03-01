using Microsoft.Extensions.Logging;

namespace Mythril.Blazor.Services;

public class FeedbackLoggerProvider(FeedbackService feedbackService) : ILoggerProvider
{
    private readonly FeedbackService _feedbackService = feedbackService;

    public ILogger CreateLogger(string categoryName) => new FeedbackLogger(_feedbackService);

    public void Dispose() { }

    private class FeedbackLogger(FeedbackService feedbackService) : ILogger
    {
        private readonly FeedbackService _feedbackService = feedbackService;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            // Fire and forget to avoid blocking UI thread
            _ = _feedbackService.CaptureError(message, exception?.StackTrace);
        }
    }
}
