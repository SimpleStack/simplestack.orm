using System.Threading.Tasks;
using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
    public class OrmConnectionFactory
    {
        private ILoggerFactory _loggerFactory;
        private ILogger<OrmConnectionFactory> _logger;
        private readonly string _connectionString;

        public OrmConnectionFactory(IDialectProvider dialectProvider, string connectionString)
        {
            DialectProvider = dialectProvider;
            _connectionString = connectionString;
            LoggerFactory = new DummyLoggerFactory();
        }

        public ILoggerFactory LoggerFactory
        {
            get => _loggerFactory;
            set
            {
                _loggerFactory = value;
                _logger = _loggerFactory.CreateLogger<OrmConnectionFactory>();
                _logger.LogInfo($"LoggingFactory initialized on OrmConnectionFactory using provider '{this}'");
            }
        }

        /// <summary>
        ///    Default command timeout (in seconds) set on OrmConnections created from the Factory
        /// </summary>
        public int DefaultCommandTimeout { get; set; }

        /// <summary>
        /// Dialect Provider assigned to this factory
        /// </summary>
        public IDialectProvider DialectProvider { get; }

        /// <summary>
        /// Open a new connection
        /// </summary>
        /// <returns>Opened connection</returns>
        public OrmConnection OpenConnection()
        {
            return OpenConnectionAsync().Result;
        }

        /// <summary>
        /// Open a new connection asynchronously
        /// </summary>
        /// <returns>Opened connection</returns>
        public async Task<OrmConnection> OpenConnectionAsync()
        {
            _logger.LogDebug("Opening connection");
            
            var conn = DialectProvider.CreateConnection(_connectionString,_loggerFactory);
            conn.CommandTimeout = DefaultCommandTimeout;
            await conn.OpenAsync().ConfigureAwait(false);
            return conn;
        }

        /// <summary>
        /// Return a string representation of the Factory.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DialectProvider.GetType().ToString();
        }
    }
}