using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;
using SimpleStack.Orm.SQLite;

namespace SimpleStack.Orm.Sqlite
{
	/// <summary>A sqlite ORM lite dialect provider base.</summary>
	public class SqliteDialectProvider : DialectProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.Sqlite.SqliteOrmLiteDialectProviderBase
		/// class.
		/// </summary>
		public SqliteDialectProvider():base(new SqliteTypeMapper())
		{
			base.SelectIdentitySql = "SELECT last_insert_rowid()";
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
		/// <param name="tableName">Name of the table.</param>
		/// <param name="schemaName">Name of the schema (optional)</param>
		/// <returns>The quoted table name.</returns>
		public override string GetQuotedTableName(string tableName, string schemaName = null)
		{
			return string.IsNullOrEmpty(schemaName) ?
				$"{EscapeChar}{NamingStrategy.GetTableName(tableName)}{EscapeChar}" :
				$"{EscapeChar}{NamingStrategy.GetTableName(schemaName)}_{NamingStrategy.GetTableName(tableName)}{EscapeChar}";
		}

		/// <summary>Query if 'dbCmd' does table exist.</summary>
		/// <param name="connection">    The database command.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		public override bool DoesTableExist(IDbConnection connection, string tableName, string schemaName = null)
		{
			string name = string.IsNullOrEmpty(schemaName) ? tableName : $"{schemaName}_{tableName}";
			
			var sql = String.Format("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = '{0}'"
				, name);

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
		public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, bool isNullable, int? fieldLength, int? scale, object defaultValue)
		{
			// Autoincrement are always INTEGER PRIMARY KEYS
			if (autoIncrement)
				return base.GetColumnDefinition(
					fieldName, 
					typeof(int), 
					true, 
					autoIncrement, 
					isNullable, 
					fieldLength,
					scale, 
					defaultValue);

			return base.GetColumnDefinition(fieldName, 
				fieldType, 
				isPrimaryKey, 
				autoIncrement, 
				isNullable, 
				fieldLength,
				scale, 
				defaultValue);
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
		            Name = c.name,
		            Definition = c.type,
		            Nullable = c.notnull == 1,
		            PrimaryKey = c.pk == 1,
		            DefaultValue = c.dflt_value,
		            DbType = DbType.String
	            };
            }
        }

        public override async Task<IEnumerable<ITableDefinition>> GetTableDefinitions(
            IDbConnection connection,
            string schemaName = null,
            bool includeViews = false)
        {
            string sqlQuery = "SELECT tbl_name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";
            
            var tables = new List<TableDefinition>();
            foreach (var table in await connection.QueryAsync(sqlQuery))
            {
	            tables.Add(new TableDefinition
	            {
		            Name = table.tbl_name,
		            SchemaName = "default"
	            });
            }

            return tables;
        }
    }
}