using System;
using Microsoft.Extensions.Logging;

namespace SimpleStack.Orm.Logging.MicrosoftExtensions
{
    public class MsLogger<T> : SimpleStack.Orm.Logging.ILogger<T>
    {
        private readonly Microsoft.Extensions.Logging.ILogger<T> _logger;

        public MsLogger(Microsoft.Extensions.Logging.ILogger<T> logger)
        {
            _logger = logger;
        }
        
        public void LogError(Exception e, string message, params object[] p)
        {
            _logger.LogError(e,message,p);
        }

        public void LogWarning(string message, params object[] p)
        {
            _logger.LogWarning(message,p);
        }

        public void LogInfo(string message, params object[] p)
        {
            _logger.LogInformation(message,p);
        }

        public void LogDebug(string message, params object[] p)
        {
            _logger.LogDebug(message,p);
        }
    }
    
    public class MsLoggerFactory : ILoggerFactory
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;

        public MsLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public ILogger<T> CreateLogger<T>()
        {
            return new MsLogger<T>(_loggerFactory.CreateLogger<T>());
        }
    }
}