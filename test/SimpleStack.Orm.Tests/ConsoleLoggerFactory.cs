using System;
using System.Diagnostics;
using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm.Tests
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        private class ConsoleLogger<T> : ILogger<T>
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
                Console.WriteLine(msg);

                var tmp = e;
                while(tmp != null)
                {
                    Console.WriteLine(tmp.Message);
                    Console.WriteLine(tmp.StackTrace);
                    tmp = tmp.InnerException;
                }
            }
        }
        public ILogger<T> CreateLogger<T>()
        {
            return new ConsoleLogger<T>();
        }
    }
}