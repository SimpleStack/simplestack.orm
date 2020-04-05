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
        ///     Default command timeout (in seconds) set on OrmConnections created from the Factory
        /// </summary>
        public int DefaultCommandTimeout { get; set; }

        public IDialectProvider DialectProvider { get; }

        public OrmConnection OpenConnection()
        {
            var conn = DialectProvider.CreateConnection(_connectionString);
            conn.CommandTimeout = DefaultCommandTimeout;
            conn.Open();
            return conn;
        }

        public async Task<OrmConnection> OpenConnectionAsync()
        {
            var conn = DialectProvider.CreateConnection(_connectionString);
            conn.CommandTimeout = DefaultCommandTimeout;
            await conn.OpenAsync();
            return conn;
        }

        public override string ToString()
        {
            return DialectProvider.GetType().ToString();
        }
    }
}