using System;

namespace SimpleStack.Orm.Logging
{
    public interface ILogger<T>
    {
        void LogError(Exception e, string message, params object[] p);
        void LogWarning(string message, params object[] p);
        void LogInfo(string message, params object[] p);
        void LogDebug(string message, params object[] p);
    }
}