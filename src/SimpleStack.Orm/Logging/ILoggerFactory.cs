namespace SimpleStack.Orm.Logging
{
    public interface ILoggerFactory
    {
        ILogger<T> CreateLogger<T>();
    }
}