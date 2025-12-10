using Microsoft.Extensions.Logging;
using CodeLeap.Application.Interfaces;

namespace CodeLeap.Infrastructure.Services;

public class LoggerService<T>(ILogger<LoggerService<T>> logger) : ILoggerService<T>
{
    public void Info(string message, params object[] args)
        => logger.LogInformation(message, args);

    public void Warning(string message, params object[] args)
        => logger.LogWarning(message, args);

    public void Error(string message, params object[] args)
        => logger.LogError(message, args);
}