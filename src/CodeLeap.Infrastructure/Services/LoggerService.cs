using Microsoft.Extensions.Logging;
using CodeLeap.Application.Interfaces;

namespace CodeLeap.Infrastructure.Services
{
    public class LoggerService<T> : ILoggerService<T>
    {
        private readonly ILogger<LoggerService<T>> _logger;

        public LoggerService(ILogger<LoggerService<T>> logger)
        {
            _logger = logger;
        }

        public void Info(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void Warning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void Error(string message, params object[] args)
            => _logger.LogError(message, args);
    }
}