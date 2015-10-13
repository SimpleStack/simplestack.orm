using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SimpleStack.Orm.MySQL;
using SimpleStack.Orm.PostgreSQL;
using SimpleStack.Orm.Sqlite;
using SimpleStack.Orm.SqlServer;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	//TODO: vdaron : Multiple Primarykey create table
	//TODO: vdaron => DateTimeOffset column types

	[TestFixture]
    public abstract partial class ExpressionTests
	{
		private readonly IDialectProvider _dialectProvider;

		public class EnumAsStringTypeHandler<T> : ITypeHandlerColumnType
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				parameter.Value = value.ToString();
			}

			public object Parse(Type destinationType, object value)
			{
				return Enum.Parse(typeof (TestEnum), (string)value);
			}

			public int? Length => (from object v in Enum.GetValues(typeof (T)) select v.ToString() into str select str.Length).Max();

			public DbType ColumnType => DbType.AnsiString;
		}

		private class EnumAsIntTypeHandler<T> : ITypeHandlerColumnType
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.Int32;
				parameter.Value = (int)value;
			}

			public object Parse(Type destinationType, object value)
			{
				return (T)value;
			}

			public int? Length => null;
			public DbType ColumnType => DbType.Int32;
		}

		public class TestType2TypeHandler : ITypeHandlerColumnType
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				parameter.Value = ((TestType2)value).TextCol;
			}

			public object Parse(Type destinationType, object value)
			{
				return new TestType2() { TextCol = value.ToString() };
			}

			public int? Length => 2000;

			public DbType ColumnType => DbType.AnsiString;
		}

		public class JsonTypeHandler : SqlMapper.ITypeHandler
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				parameter.Value = NServiceKit.Text.JsonSerializer.SerializeToString(value);
			}

			public object Parse(Type destinationType, object value)
			{
				return NServiceKit.Text.JsonSerializer.DeserializeFromString(value.ToString(), destinationType);
			}
		}

		private OrmConnection _conn;

		protected ExpressionTests(IDialectProvider dialectProvider)
		{
			_dialectProvider = dialectProvider;
			Logging.LogProvider.SetCurrentLogProvider(new ColoredConsoleLogProvider());
		}

		[SetUp]
		public void Setup()
		{
			if (_conn != null)
			{
				_conn.Dispose();
				_conn = null;
			}

			SqlMapper.ResetTypeHandlers();

			SqlMapper.AddTypeHandler(typeof(TestEnum), new EnumAsIntTypeHandler<TestEnum>());
			SqlMapper.AddTypeHandler(typeof(TestType2),new TestType2TypeHandler());
			
			OpenDbConnection().CreateTable<TestType>(true);
			OpenDbConnection().CreateTable<Person>(true);
			OpenDbConnection().CreateTable<TestType2>(true);

		}

		protected abstract string ConnectionString { get; }

		protected OrmConnection OpenDbConnection()
		{
			if (_conn?.DbConnection == null)
			{
				_conn = _dialectProvider.CreateConnection(ConnectionString);
				_conn.Open();
			}
			return _conn;
		}

		/// <summary>Establish context.</summary>
		/// <param name="numberOfRandomObjects">Number of random objects.</param>
		protected void EstablishContext(int numberOfRandomObjects)
		{
			EstablishContext(numberOfRandomObjects, null);
		}

		/// <summary>Establish context.</summary>
		/// <param name="numberOfRandomObjects">Number of random objects.</param>
		/// <param name="obj">                  A variable-length parameters list containing object.</param>
		protected void EstablishContext(int numberOfRandomObjects, params TestType[] obj)
		{
			if (obj == null)
				obj = new TestType[0];

			var con = OpenDbConnection();
			{
				foreach (var t in obj)
				{
					con.Insert(t);
				}

				var random = new Random((int)(DateTime.UtcNow.Ticks ^ (DateTime.UtcNow.Ticks >> 4)));
				for (var i = 0; i < numberOfRandomObjects; i++)
				{
					TestType o = null;

					while (o == null)
					{
						int intVal = random.Next();

						o = new TestType
						{
							BoolColumn = random.Next() % 2 == 0,
							IntColumn = intVal,
							StringColumn = Guid.NewGuid().ToString()
						};

						if (obj.Any(x => x.IntColumn == intVal))
							o = null;
					}

					con.Insert(o);
				}
			}
		}

		/// <summary>Gets a value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="item">The item.</param>
		/// <returns>The value.</returns>
		public T GetValue<T>(T item)
		{
			return item;
		}
	}

	public class PostgreSQLTests : ExpressionTests
	{
		public PostgreSQLTests() : base(new PostgreSQLDialectProvider())
		{
		}

		protected override string ConnectionString => "server=localhost;user id=postgres;password=depfac$2000;database=test;Enlist=true";
	}
	public class MySQLTests : ExpressionTests
	{
		protected override string ConnectionString => "server=localhost;user=root;password=depfac$2000;database=df_core_test";

		public MySQLTests() : base(new MySqlDialectProvider())
		{
		}
	}
	public class SQLServerTests : ExpressionTests
	{
		public SQLServerTests() : base(new SqlServerDialectProvider())
		{
		}

		protected override string ConnectionString => @"server=.\SqlExpress;Trusted_Connection=true;database=testdb";
	}
	public class SQLLiteTests : ExpressionTests
	{
		public SQLLiteTests() : base(new SqliteDialectProvider
		{
			BinaryGUID = false,
			Compress = true,
			UTF8Encoded = true,
			DateTimeFormatAsTicks = false
		})
		{
		}

		protected override string ConnectionString => @"Data Source=e:\mydb.db;Version=3;New=True;";
		//protected override string ConnectionString => @":memory:";
	}
}
