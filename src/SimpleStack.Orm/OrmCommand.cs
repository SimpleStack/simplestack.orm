using System.Data;
using System.Data.Common;

//using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
    internal class OrmCommand : DbCommand
    {
        //private static readonly ILog Logger = LogProvider.For<OrmCommand>();

        private readonly DbCommand _command;

        internal OrmCommand(DbCommand command)
        {
            //	Logger.Debug("Creating Command");
            _command = command;
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
            //	Logger.Debug("Preparing command");
            _command.Prepare();
        }

        public override void Cancel()
        {
            //Logger.DebugFormat("Cancelling command: {0}", CommandText);
            _command.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            //Logger.DebugFormat("Before ExecuteDbDataReader : {0}",CommandText);
            var res = _command.ExecuteReader(behavior);
            //Logger.Debug("After ExecuteDbDataReader");
            return res;
        }

        public override int ExecuteNonQuery()
        {
            var res = _command.ExecuteNonQuery();
            //Logger.Debug("After ExecuteNonQuery");
            return res;
        }

        public override object ExecuteScalar()
        {
            //Logger.DebugFormat("Before ExecuteScalar: {0}", CommandText);
            var res = _command.ExecuteScalar();
            //Logger.Debug("After ExecuteScalar");
            return res;
        }
    }
}