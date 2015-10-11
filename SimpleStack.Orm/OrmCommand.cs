using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStack.Orm
{
	class OrmCommand : DbCommand
	{
		private readonly DbCommand _command;

		internal OrmCommand(DbCommand command)
		{
			_command = command;
		}
		
		public override void Prepare()
		{
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
			_command.Cancel();
		}

		protected override DbParameter CreateDbParameter()
		{
			return _command.CreateParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return _command.ExecuteReader(behavior);
		}

		public override int ExecuteNonQuery()
		{
			return _command.ExecuteNonQuery();
		}

		public override object ExecuteScalar()
		{
			return _command.ExecuteScalar();
		}
	}
}
