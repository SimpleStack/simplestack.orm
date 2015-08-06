using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStack.Orm
{
	class OrmCommand : IDbCommand
	{
		private readonly IDbCommand _command;

		internal OrmCommand(IDbCommand command)
		{
			_command = command;
		}

		public void Dispose()
		{
			_command.Dispose();
		}

		public void Prepare()
		{
			_command.Prepare();
		}

		public void Cancel()
		{
			_command.Cancel();
		}

		public IDbDataParameter CreateParameter()
		{
			return _command.CreateParameter();
		}

		public int ExecuteNonQuery()
		{
			return _command.ExecuteNonQuery();
		}

		public IDataReader ExecuteReader()
		{
			return _command.ExecuteReader();
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return _command.ExecuteReader(behavior);
		}

		public object ExecuteScalar()
		{
			return _command.ExecuteScalar();
		}

		public IDbConnection Connection
		{
			get { return _command.Connection; } 
			set { _command.Connection = value; }
		}

		public IDbTransaction Transaction
		{
			get { return _command.Transaction; }
			set { _command.Transaction = value; }
		}
		public string CommandText
		{
			get { return _command.CommandText; }
			set { _command.CommandText = value; }
		}
		public int CommandTimeout
		{
			get { return _command.CommandTimeout; }
			set { _command.CommandTimeout= value; }
		}
		public CommandType CommandType
		{
			get { return _command.CommandType; }
			set { _command.CommandType= value; }
		}

		public IDataParameterCollection Parameters => _command.Parameters;

		public UpdateRowSource UpdatedRowSource
		{
			get { return _command.UpdatedRowSource; }
			set { _command.UpdatedRowSource = value; }
		}
	}
}
