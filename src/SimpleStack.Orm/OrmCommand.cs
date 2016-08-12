using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
	class OrmCommand : DbCommand
	{
		//private static readonly ILog Logger = LogProvider.For<OrmCommand>();

		private readonly DbCommand _command;

		internal OrmCommand(DbCommand command)
		{
		//	Logger.Debug("Creating Command");
			_command = command;
		}
		
		public override void Prepare()
		{
		//	Logger.Debug("Preparing command");
			_command.Prepare();
		}

		public override string CommandText { get { return _command.CommandText; } set { _command.CommandText = value; } }
		public override int CommandTimeout { get { return _command.CommandTimeout; } set { _command.CommandTimeout = value; } }
		public override CommandType CommandType { get { return _command.CommandType; } set { _command.CommandType = value; } }
		public override UpdateRowSource UpdatedRowSource { get { return _command.UpdatedRowSource; } set { _command.UpdatedRowSource = value; } }
		protected override DbConnection DbConnection { get { return _command.Connection; } set { _command.Connection = value; } }
		protected override DbParameterCollection DbParameterCollection => _command.Parameters;
		protected override DbTransaction DbTransaction { get { return _command.Transaction; } set { _command.Transaction = value; } }
		public override bool DesignTimeVisible { get { return _command.DesignTimeVisible; } set { _command.DesignTimeVisible = value; } }

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
			//Logger.DebugFormat("Before ExecuteNonQuery: {0}", CommandText);
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
