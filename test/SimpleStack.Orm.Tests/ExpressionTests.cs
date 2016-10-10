using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
#if NET45 || NET451
using SimpleStack.Orm.MySQL;
#endif
using SimpleStack.Orm.PostgreSQL;
using SimpleStack.Orm.Sqlite;
using SimpleStack.Orm.SqlServer;
using SimpleStack.Orm.MySQLConnector;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	//TODO: vdaron : Multiple Primarykey create table
	//TODO: vdaron => DateTimeOffset column types

	[TestFixture]
    public abstract partial class ExpressionTests
	{
		private readonly IDialectProvider _dialectProvider;

		public class GuidAsByteArray : ITypeHandlerColumnType
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.Binary;
				parameter.Value = ((Guid)value).ToByteArray();
			}

			public object Parse(Type destinationType, object value)
			{
				return new Guid((byte[])value);
			}

			public int? Length => (Guid.Empty.ToByteArray().Length);

			public DbType ColumnType => DbType.Binary;
		}

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

		public class JsonTypeHandler : SqlMapper.ITypeHandler
		{
			private JsonSerializer s = new JsonSerializer();

			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				using (var writer = new StringWriter())
				using (var rr = new JsonTextWriter(writer))
				{
					s.Serialize(rr,value);
					parameter.Value = rr.ToString();
				}
			}

			public object Parse(Type destinationType, object value)
			{
				using (var reader = new StringReader(value.ToString()))
				using (var rr = new JsonTextReader(reader))
				{
					return s.Deserialize(rr, destinationType);
				}
			}
		}

		private OrmConnection _conn;

		protected ExpressionTests(IDialectProvider dialectProvider)
		{
			_dialectProvider = dialectProvider;
			//BasicConfigurator.Configure(new DebugAppender{Layout =  new SimpleLayout()});
		}

		[SetUp]
		public virtual void Setup()
		{
			if (_conn != null)
			{
				_conn.Dispose();
				_conn = null;
			}

			SqlMapper.ResetTypeHandlers();

			SqlMapper.AddTypeHandler(typeof(TestEnum), new EnumAsIntTypeHandler<TestEnum>());
			
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

			using (var con = OpenDbConnection())
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
#if NET45 || NET451
	public class MySQLTests : ExpressionTests
	{
		protected override string ConnectionString => "server=localhost;user=root;password=depfac$2000;database=test";

		public MySQLTests() : base(new MySqlDialectProvider())
		{
		}
	}
#endif
	public class MySQLConnectorTests : ExpressionTests
	{
		protected override string ConnectionString => "server=localhost;user=root;password=depfac$2000;database=test";

		public MySQLConnectorTests() : base(new MySqlConnectorDialectProvider())
		{
		}
	}
	public class SQLServerTests : ExpressionTests
	{
		public SQLServerTests() : base(new SqlServerDialectProvider())
		{
		}

		protected override string ConnectionString => @"server=.\SqlExpress;Trusted_Connection=true;database=test";
	}
	public class SQLLiteTests : ExpressionTests
	{
		private Microsoft.Data.Sqlite.SqliteConnectionStringBuilder builder;
		public SQLLiteTests() : base(new SqliteDialectProvider())
		{
			builder = new SqliteConnectionStringBuilder();
			builder.DataSource = Path.Combine(Path.GetTempPath(),"test.db");
			builder.Mode = SqliteOpenMode.ReadWriteCreate;
			builder.Cache = SqliteCacheMode.Shared;

			
		}

		public override void Setup()
		{
			base.Setup();

			SqlMapper.AddTypeHandler(typeof(Guid), new GuidAsByteArray());
		}

		protected override string ConnectionString => builder.ToString();
	}
}
