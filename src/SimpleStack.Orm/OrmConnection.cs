using System.Data;
using System.Data.Common;
using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
    public partial class OrmConnection : DbConnection
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<OrmConnection> _logger;
        private bool _isOpen;

        /// <summary>
        /// Initializes a new instance of the OrmLiteConnection class.
        /// </summary>
        /// <param name="connection">The inner Connection</param>
        /// <param name="dialectProvider">The dialect provider</param>
        internal OrmConnection(DbConnection connection, IDialectProvider dialectProvider, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            DialectProvider = dialectProvider;
            DbConnection = connection;
            
            _logger = loggerFactory.CreateLogger<OrmConnection>();
        }

        /// <summary>
        /// The Dialect Provider attached to this connection
        /// </summary>
        public IDialectProvider DialectProvider { get; }

        /// <summary>
        /// Command timeout for command created from this connection
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// The current transaction attached to this connection
        /// </summary>
        public OrmTransaction Transaction { get; internal set; }

        /// <summary>
        /// Internal DbConnection object
        /// </summary>
        public DbConnection DbConnection { get; private set; }
        
        /// <inheritdoc />
        public override string ConnectionString
        {
            get => DbConnection.ConnectionString;
            set => DbConnection.ConnectionString = value;
        }
        
        /// <inheritdoc />
        public override string Database => DbConnection.Database;
        
        /// <inheritdoc />
        public override ConnectionState State => DbConnection.State;
        
        /// <inheritdoc />
        public override string DataSource => DbConnection.DataSource;
        
        /// <inheritdoc />
        public override string ServerVersion => DbConnection.ServerVersion;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbConnection != null)
                {
                    _logger.LogDebug("Closing connection");
                    Close();

                    DbConnection.Dispose();
                    DbConnection = null;
                }

                _isOpen = false;
            }

            base.Dispose(disposing);
        }

        internal void ClearCurrentTransaction()
        {
            Transaction = null;
        }
        
        /// <inheritdoc />
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            Transaction = new OrmTransaction(this, DbConnection.BeginTransaction());
            return Transaction;
        }


        /// <inheritdoc />
        public override void Close()
        {
            if (_isOpen)
            {
                DbConnection.Close();
                _isOpen = false;
            }
        }


        /// <inheritdoc />
        public override void ChangeDatabase(string databaseName)
        {
            DbConnection.ChangeDatabase(databaseName);
        }


        /// <inheritdoc />
        public override void Open()
        {
            DbConnection.Open();
            _isOpen = true;
        }

        /// <inheritdoc />
        public override DataTable GetSchema()
        {
            return DbConnection.GetSchema();
        }
        
        /// <inheritdoc />
        public override DataTable GetSchema(string collectionName)
        {
            return DbConnection.GetSchema(collectionName);
        }
        
        /// <inheritdoc />
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return DbConnection.GetSchema(collectionName, restrictionValues);
        }
        
        /// <inheritdoc />
        protected override DbCommand CreateDbCommand()
        {
            var cmd = new OrmCommand(DbConnection.CreateCommand(), _loggerFactory);
            if (Transaction != null)
            {
                cmd.Transaction = Transaction.trans;
            }

            cmd.CommandTimeout = CommandTimeout;
            return cmd;
        }
    }
}