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

		/// <summary>Convert database value.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="value">The value.</param>
		/// <param name="type"> The type.</param>
		/// <returns>The database converted value.</returns>
		//public override object ConvertDbValue(object value, Type type)
		//{
		//	if (value == null || value is DBNull) return null;

		//	if (type == typeof(bool) && !(value is bool))
		//	{
		//		var intVal = int.Parse(value.ToString());
		//		return intVal != 0;
		//	}
		//	if (type == typeof(TimeSpan))
		//	{
		//		var dateValue = value as DateTime?;
		//		if (dateValue != null)
		//		{
		//			var now = DateTime.Now;
		//			var todayWithoutTime = new DateTime(now.Year, now.Month, now.Day);
		//			var ts = dateValue.Value - todayWithoutTime;
		//			return ts;
		//		}
		//	}

		//	// support for parsing datetime offset
		//	if(type == typeof(DateTimeOffset))
		//	{
		//		var moment = DateTimeOffset.Parse((string)value, null, DateTimeStyles.RoundtripKind);
		//		return moment;
		//	}

		//	try
		//	{
		//		return base.ConvertDbValue(value, type);
		//	}
		//	catch (Exception ex)
		//	{
		//		throw;
		//	}
		//}

		/// <summary>Gets quoted value.</summary>
		/// <param name="value">    The value.</param>
		/// <param name="fieldType">Type of the field.</param>
		/// <returns>The quoted value.</returns>
		//public override string GetQuotedValue(object value, Type fieldType)
		//{
		//	if (value == null) return "NULL";

		//	if (fieldType == typeof(Guid))
		//	{
		//		var guidValue = (Guid)value;
		//		return base.GetQuotedValue(guidValue.ToString("N"), typeof(string));
		//	}
		//	//if (fieldType == typeof(DateTime))
		//	//{
		//	//	var dateValue = (DateTime)value;
		//	//	return base.GetQuotedValue(
		//	//		DateTimeSerializer.ToShortestXsdDateTimeString(dateValue),
		//	//		typeof(string));
		//	//}
		//	if (fieldType == typeof(bool))
		//	{
		//		var boolValue = (bool)value;
		//		return base.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
		//	}

		//	// output datetimeoffset as a string formatted for roundtripping.
		//	if (fieldType == typeof (DateTimeOffset))
		//	{
		//		var dateTimeOffsetValue = (DateTimeOffset) value;
		//		return base.GetQuotedValue(dateTimeOffsetValue.ToString("o"), typeof (string));
		//	}

		//	return base.GetQuotedValue(value, fieldType);
		//}

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
	}
}