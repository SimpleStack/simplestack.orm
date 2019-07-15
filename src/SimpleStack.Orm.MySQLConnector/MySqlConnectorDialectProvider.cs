using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;
using MySql.Data.MySqlClient;

namespace SimpleStack.Orm.MySQLConnector
{
	/// <summary>my SQL dialect provider.</summary>
	public class MySqlConnectorDialectProvider : DialectProviderBase
	{
		/// <summary>The instance.</summary>
		public static MySqlConnectorDialectProvider Instance = new MySqlConnectorDialectProvider();

		/// <summary>The text column definition.</summary>
		private const string TextColumnDefinition = "TEXT";

		/// <summary>
		/// Prevents a default instance of the NServiceKit.OrmLite.MySql.MySqlDialectProvider class from
		/// being created.
		/// </summary>
		public MySqlConnectorDialectProvider()
		{
			base.AutoIncrementDefinition = "AUTO_INCREMENT";
			base.IntColumnDefinition = "int(11)";
			base.BoolColumnDefinition = "tinyint(1)";
			base.TimeColumnDefinition = "time";
			base.DecimalColumnDefinition = "decimal(38,6)";
			base.GuidColumnDefinition = "CHAR(36)";// "char(32)" //TODO: Fix Guid length in MySQL
			base.DefaultStringLength = 255;
			base.InitColumnTypeMap();
			base.DefaultValueFormat = " DEFAULT '{0}'";
			base.SelectIdentitySql = "SELECT LAST_INSERT_ID()";
		}

		/// <summary>Creates a connection.</summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="options">         Options for controlling the operation.</param>
		/// <returns>The new connection.</returns>
		public override DbConnection CreateIDbConnection(string connectionString)
		{
			var c = new MySqlConnection(connectionString);
			return c;

		}

		/// <summary>Gets quoted table name.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The quoted table name.</returns>
		public override string GetQuotedTableName(ModelDefinition modelDef)
		{
			return string.Format("`{0}`", NamingStrategy.GetTableName(modelDef.ModelName));
		}

		/// <summary>Gets quoted table name.</summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>The quoted table name.</returns>
		public override string GetQuotedTableName(string tableName)
		{
			return string.Format("`{0}`", NamingStrategy.GetTableName(tableName));
		}

		/// <summary>Gets quoted column name.</summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The quoted column name.</returns>
		public override string GetQuotedColumnName(string columnName)
		{
			return string.Format("`{0}`", NamingStrategy.GetColumnName(columnName));
		}

		/// <summary>Gets quoted name.</summary>
		/// <param name="name">The name.</param>
		/// <returns>The quoted name.</returns>
		public override string GetQuotedName(string name)
		{
			return string.Format("`{0}`", name);
		}

		/// <summary>Query if 'dbCmd' does table exist.</summary>
		/// <param name="connection">    The database command.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		public override bool DoesTableExist(IDbConnection connection, string tableName)
		{
			//Same as SQL Server apparently?
			var sql = String.Format("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
				"WHERE TABLE_NAME = '{0}' AND " +
				"TABLE_SCHEMA = '{1}'", tableName, connection.Database);

			//if (!string.IsNullOrEmpty(schemaName))
			//    sql += " AND TABLE_SCHEMA = {0}".SqlFormat(schemaName);

			var result = connection.ExecuteScalar<long>(sql);

			return result > 0;
		}

		/// <summary>Gets column definition.</summary>
		/// <param name="fieldDefinition">The field definition.</param>
		/// <returns>The column definition.</returns>
		public string GetColumnDefinition(FieldDefinition fieldDefinition)
		{
			if (fieldDefinition.PropertyInfo.FirstAttribute<TextAttribute>() != null)
			{
				var sql = new StringBuilder();
				sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDefinition.FieldName), TextColumnDefinition);
				sql.Append(fieldDefinition.IsNullable ? " NULL" : " NOT NULL");
				return sql.ToString();
			}

			return base.GetColumnDefinition(
				 fieldDefinition.FieldName,
				 fieldDefinition.FieldType,
				 fieldDefinition.IsPrimaryKey,
				 fieldDefinition.AutoIncrement,
				 fieldDefinition.IsNullable,
				 fieldDefinition.FieldLength,
				 null,
				 fieldDefinition.DefaultValue);
		}

		public override string GetDatePartFunction(string name, string quotedColName)
		{
			switch (name)
			{
				case "Year":
					return $"YEAR({quotedColName})";
				case "Month":
					return $"MONTH({quotedColName})";
				case "Day":
					return $"DAY({quotedColName})";
				case "Hour":
					return $"HOUR({quotedColName})";
				case "Minute":
					return $"MINUTE({quotedColName})";
				case "Second":
					return $"SECOND({quotedColName})";
				case "Quarter":
					return $"QUARTER({quotedColName})";
				default:
					throw new NotImplementedException();
			}
		}

        //In this case SchemaName = dbName
        public override IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection, string tableName, string schemaName = null)
        {
            string sqlQuery = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @TableName";
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                sqlQuery += " AND TABLE_SCHEMA = @SchemaName";
            }
            foreach (var c in connection.Query(sqlQuery, new { TableName = tableName, SchemaName = schemaName }))
            {
                yield return new ColumnDefinition
                {
                    Name = c.COLUMN_NAME,
                    Type = c.DATA_TYPE,
                    DefaultValue = c.COLUMN_DEFAULT,
                    Nullable = c.IS_NULLABLE == "YES",
                    PrimaryKey = c.COLUMN_KEY == "PRI",
                    FieldLength = (int?)c.CHARACTER_MAXIMUM_LENGTH
                };
            }
        }

        public override IEnumerable<ITableDefinition> GetTableDefinitions(IDbConnection connection,string schemaName = null)
        {
            string sqlQuery = "SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                sqlQuery += " AND TABLE_SCHEMA=@SchemaName";
            }
            foreach (var table in connection.Query(sqlQuery, new {SchemaName = schemaName}))
            {
                yield return new TableDefinition { Name = table.TABLE_NAME, SchemaName = table.TABLE_SCHEMA };
            }
        }
    }
}
