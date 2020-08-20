using System;
using System.Diagnostics;

namespace SimpleStack.Orm.Logging
{
    public class DebugLoggerFactory : ILoggerFactory
    {
        private class DebugLogger<T> : ILogger<T>
        {
            public void LogError(Exception e, string message, params object[] p)
            {
                Log("ERROR",message,p,e);
            }

            public void LogWarning(string message, params object[] p)
            {
                Log("WARN",message,p,null);
            }

            public void LogInfo(string message, params object[] p)
            {
                Log("INFO",message,p,null);
            }

            public void LogDebug(string message, params object[] p)
            {
                Log("DEBUG",message,p,null);
            }

            private void Log(string type, string message, object[] p, Exception e)
            {
                var msg = $"{DateTime.Now:O} - [{type}] - {string.Format(message, p)}";
                Debug.WriteLine(msg);

                var tmp = e;
                while(tmp != null)
                {
                    Debug.WriteLine(tmp.Message);
                    Debug.WriteLine(tmp.StackTrace);
                    tmp = tmp.InnerException;
                }
            }
        }
        public ILogger<T> CreateLogger<T>()
        {
            return new DebugLogger<T>();
        }
    }
}