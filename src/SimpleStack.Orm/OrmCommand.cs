using System.Data;
using System.Data.Common;
using SimpleStack.Orm.Logging;

//using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
    internal class OrmCommand : DbCommand
    {
        private ILogger<OrmCommand> _logger;

        private readonly DbCommand _command;
        private readonly ILoggerFactory _loggerFactory;

        internal OrmCommand(DbCommand command, ILoggerFactory loggerFactory)
        {
            _command = command;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<OrmCommand>();
        }

        public override string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _command.Connection;
            set => _command.Connection = value;
        }

        protected override DbParameterCollection DbParameterCollection => _command.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _command.DesignTimeVisible;
            set => _command.DesignTimeVisible = value;
        }

        public override void Prepare()
        {
            _logger.LogDebug("Preparing command");
            _command.Prepare();
        }

        public override void Cancel()
        {
            _logger.LogDebug("Cancelling command: {0}", CommandText);
            _command.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            _logger.LogDebug("Executing ExecuteReader : {0}", CommandText);
            var res = _command.ExecuteReader(behavior);
            _logger.LogDebug("ExecuteReader complete");
            return res;
        }

        public override int ExecuteNonQuery()
        {
            _logger.LogDebug("Executing ExecuteNonQuery : {0}", CommandText);
            var res = _command.ExecuteNonQuery();
            _logger.LogDebug("ExecuteNonQuery complete with result '{0}'", res);
            return res;
        }

        public override object ExecuteScalar()
        {
            _logger.LogDebug("Executing ExecuteScalar: {0}", CommandText);
            var res = _command.ExecuteScalar();
            _logger.LogDebug("ExecuteScalar complete with result '{0}'", res);
            return res;
        }
    }
}