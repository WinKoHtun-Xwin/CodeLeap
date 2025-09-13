namespace CodeLeap.Application.Interfaces
{
    public interface ILoggerService<T>
    {
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(string message, params object[] args);
    }
}
