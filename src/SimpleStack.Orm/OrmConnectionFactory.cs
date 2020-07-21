using System.Threading.Tasks;

namespace SimpleStack.Orm
{
    public class OrmConnectionFactory
    {
        private readonly string _connectionString;

        public OrmConnectionFactory(IDialectProvider dialectProvider, string connectionString)
        {
            DialectProvider = dialectProvider;
            _connectionString = connectionString;
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
            var conn = DialectProvider.CreateConnection(_connectionString);
            conn.CommandTimeout = DefaultCommandTimeout;
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Open a new connection asynchronously
        /// </summary>
        /// <returns>Opened connection</returns>
        public async Task<OrmConnection> OpenConnectionAsync()
        {
            var conn = DialectProvider.CreateConnection(_connectionString);
            conn.CommandTimeout = DefaultCommandTimeout;
            await conn.OpenAsync();
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