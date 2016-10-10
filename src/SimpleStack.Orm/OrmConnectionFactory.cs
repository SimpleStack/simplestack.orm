using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStack.Orm
{
	public class OrmConnectionFactory
	{
		private readonly IDialectProvider _dialectProvider;
		private readonly string _connectionString;

		public OrmConnectionFactory(IDialectProvider dialectProvider, string connectionString)
		{
			_dialectProvider = dialectProvider;
			_connectionString = connectionString;
		}

		public OrmConnection OpenConnection()
		{
			var conn = _dialectProvider.CreateConnection(_connectionString);
			conn.Open();
			return conn;
		}

		public async Task<OrmConnection> OpenConnectionAsync()
		{
			var conn = _dialectProvider.CreateConnection(_connectionString);
			await conn.OpenAsync();
			return conn;
		}
	}
}
