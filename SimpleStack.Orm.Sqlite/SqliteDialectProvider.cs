using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using Dapper;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.Sqlite
{
	/// <summary>A sqlite ORM lite dialect provider base.</summary>
	public class SqliteDialectProvider : DialectProviderBase<SqliteDialectProvider>
	{
		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.Sqlite.SqliteOrmLiteDialectProviderBase
		/// class.
		/// </summary>
		public SqliteDialectProvider()
		{
			base.DateTimeColumnDefinition = base.StringColumnDefinition;
			base.BoolColumnDefinition = base.IntColumnDefinition;
			base.GuidColumnDefinition = "GUID";
			base.SelectIdentitySql = "SELECT last_insert_rowid()";

			base.InitColumnTypeMap();

			// add support for DateTimeOffset
			DbTypeMap.Set(DbType.DateTimeOffset, StringColumnDefinition);
			DbTypeMap.Set(DbType.DateTimeOffset, StringColumnDefinition);
		}

		/// <summary>Gets or sets the password.</summary>
		/// <value>The password.</value>
		public string Password { get; set; }

		/// <summary>Gets or sets a value indicating whether the UTF 8 encoded.</summary>
		/// <value>true if UTF 8 encoded, false if not.</value>
		public bool UTF8Encoded { get; set; }

		public bool BinaryGUID { get; set; }

		public bool Compress { get; set; }

		public bool DateTimeFormatAsTicks { get; set; }

		///// <summary>Gets or sets a value indicating whether the parse via framework.</summary>
		///// <value>true if parse via framework, false if not.</value>
		//public static bool ParseViaFramework { get; set; }

		/// <summary>Creates full text create table statement.</summary>
		/// <param name="objectWithProperties">The object with properties.</param>
		/// <returns>The new full text create table statement.</returns>
		public static string CreateFullTextCreateTableStatement(object objectWithProperties)
		{
			var sbColumns = new StringBuilder();
			foreach (var propertyInfo in objectWithProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var columnDefinition = (sbColumns.Length == 0)
					? string.Format("{0} TEXT PRIMARY KEY", propertyInfo.Name)
					: string.Format(", {0} TEXT", propertyInfo.Name);

				sbColumns.AppendLine(columnDefinition);
			}

			var tableName = objectWithProperties.GetType().Name;
			var sql = string.Format("CREATE VIRTUAL TABLE \"{0}\" USING FTS3 ({1});", tableName, sbColumns);

			return sql;
		}

		/// <summary>Creates a connection.</summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="options">         Options for controlling the operation.</param>
		/// <returns>The new connection.</returns>
		public override DbConnection CreateIDbConnection(string connectionString)
		{
			var isFullConnectionString = connectionString.Contains(";");
			var connString = new StringBuilder();
			if (!isFullConnectionString)
			{
				if (connectionString != ":memory:")
				{
					var existingDir = Path.GetDirectoryName(connectionString);
					if (!string.IsNullOrEmpty(existingDir) && !Directory.Exists(existingDir))
					{
						Directory.CreateDirectory(existingDir);
					}
				}
				connString.AppendFormat(@"Data Source={0};Version=3;New=True;", connectionString.Trim());

			}
			else
			{
				connString.Append(connectionString);
			}
			if (!string.IsNullOrEmpty(Password))
			{
				connString.AppendFormat("Password={0};", Password);
			}
			if (UTF8Encoded)
			{
				connString.Append("UseUTF16Encoding=True;");
			}
			if (Compress)
			{
				connString.Append("Compress=True;");
			}
			if (BinaryGUID)
			{
				connString.Append("BinaryGUID=True;");
			}
			if (DateTimeFormatAsTicks)
			{
				connString.Append("DateTimeFormat=Ticks;");
			}

			return new System.Data.SQLite.SQLiteConnection(connString.ToString());
		}

		/// <summary>Gets quoted table name.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The quoted table name.</returns>
		public override string GetQuotedTableName(ModelDefinition modelDef)
		{
			if (!modelDef.IsInSchema)
				return base.GetQuotedTableName(modelDef);

			return string.Format("\"{0}_{1}\"", modelDef.Schema, modelDef.ModelName);
		}

		/// <summary>Expression visitor.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public override SqlExpressionVisitor<T> ExpressionVisitor<T>()
		{
			return new SqliteExpressionVisitor<T>(this);
		}

		/// <summary>Query if 'dbCmd' does table exist.</summary>
		/// <param name="connection">    The database command.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		public override bool DoesTableExist(IDbConnection connection, string tableName)
		{
			var sql = String.Format("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = '{0}'"
				, tableName);

			var result = connection.ExecuteScalar<int>(sql);

			return result > 0;
		}

		/// <summary>Gets column definition.</summary>
		/// <param name="fieldName">    Name of the field.</param>
		/// <param name="fieldType">    Type of the field.</param>
		/// <param name="isPrimaryKey"> true if this object is primary key.</param>
		/// <param name="autoIncrement">true to automatically increment.</param>
		/// <param name="isNullable">   true if this object is nullable.</param>
		/// <param name="fieldLength">  Length of the field.</param>
		/// <param name="scale">        The scale.</param>
		/// <param name="defaultValue"> The default value.</param>
		/// <returns>The column definition.</returns>
		public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, bool isNullable, int? fieldLength, int? scale, string defaultValue)
		{
			// http://www.sqlite.org/lang_createtable.html#rowid
			var ret = base.GetColumnDefinition(fieldName, fieldType, isPrimaryKey, autoIncrement, isNullable, fieldLength, scale, defaultValue);
			if (isPrimaryKey)
				return ret.Replace(" BIGINT ", " INTEGER ");
			return ret;
		}

		public override string GetDatePartFunction(string name, string quotedColName)
		{
			switch (name)
			{
				case "Year":
					return $"strftime('%Y',{quotedColName})";
				case "Month":
					return $"strftime('%m',{quotedColName})";
				case "Day":
					return $"strftime('%d',{quotedColName})";
				case "Hour":
					return $"strftime('%H',{quotedColName})";
				case "Minute":
					return $"strftime('%M',{quotedColName})";
				case "Second":
					return $"strftime('%S',{quotedColName})";
				case "Quarter":
					return $"(((strftime('%m',{quotedColName})- 1) / 3) + 1)";
				default:
					throw new NotImplementedException();
			}
		}
	}
}