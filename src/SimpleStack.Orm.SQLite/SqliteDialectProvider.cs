using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.Sqlite
{
	/// <summary>A sqlite ORM lite dialect provider base.</summary>
	public class SqliteDialectProvider : DialectProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.Sqlite.SqliteOrmLiteDialectProviderBase
		/// class.
		/// </summary>
		public SqliteDialectProvider()
		{
			base.DateTimeColumnDefinition = base.StringColumnDefinition;
			base.BoolColumnDefinition = base.IntColumnDefinition;
			//base.GuidColumnDefinition = "GUID";
			base.SelectIdentitySql = "SELECT last_insert_rowid()";

			base.InitColumnTypeMap();

			// add support for DateTimeOffset
			DbTypeMap.Set(DbType.DateTimeOffset, StringColumnDefinition);
			DbTypeMap.Set(DbType.DateTimeOffset, StringColumnDefinition);
		}

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
			return new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
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
					return $"CAST(strftime('%Y',{quotedColName}) AS INT)";
				case "Month":
					return $"CAST(strftime('%m',{quotedColName}) AS INT)";
				case "Day":
					return $"CAST(strftime('%d',{quotedColName}) AS INT)";
				case "Hour":
					return $"CAST(strftime('%H',{quotedColName}) AS INT)";
				case "Minute":
					return $"CAST(strftime('%M',{quotedColName}) AS INT)";
				case "Second":
					return $"CAST(strftime('%S',{quotedColName}) AS INT)";
				case "Quarter":
					return $"CAST((((strftime('%m',{quotedColName})- 1) / 3) + 1) AS INT)";
				default:
					throw new NotImplementedException();
			}
		}

        public override IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection, string tableName, string schemaName = null)
        {
            string sqlQuery = $"pragma table_info('{tableName}')";
            foreach (var c in connection.Query(sqlQuery))
            {
                yield return new ColumnDefinition
                             {
                                 Name         = c.name,
                                 Type         = c.type,
                                 Nullable     = c.notnull == 1,
                                 PrimaryKey   = c.pk == 1,
                                 DefaultValue = c.dflt_value
                             };
            }
        }

        public override IEnumerable<ITableDefinition> GetTableDefinitions(
            IDbConnection connection,
            string schemaName = null)
        {
            string sqlQuery = "SELECT tbl_name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";
            foreach (var table in connection.Query(sqlQuery))
            {
                yield return new TableDefinition
                             {
                                 Name = table.tbl_name,
                                 SchemaName = "default"
                             };
            }
        }
    }
}