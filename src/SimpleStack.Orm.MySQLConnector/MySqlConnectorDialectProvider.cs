using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;
using MySql.Data.MySqlClient;
using SimpleStack.Orm.MySQL;

namespace SimpleStack.Orm.MySQLConnector
{
	/// <summary>my SQL dialect provider.</summary>
	public class MySqlConnectorDialectProvider : DialectProviderBase
	{
				/// <summary>
		/// Prevents a default instance of the NServiceKit.OrmLite.MySql.MySqlDialectProvider class from
		/// being created.
		/// </summary>
		public MySqlConnectorDialectProvider():base(new MySqlConnectorTypeMapper())
		{
			base.AutoIncrementDefinition = "AUTO_INCREMENT";
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

//		/// <summary>Gets column definition.</summary>
//		/// <param name="fieldDefinition">The field definition.</param>
//		/// <returns>The column definition.</returns>
//		public string GetColumnDefinition(FieldDefinition fieldDefinition)
//		{
//			if (fieldDefinition.PropertyInfo.FirstAttribute<TextAttribute>() != null)
//			{
//				var sql = new StringBuilder();
//				sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDefinition.FieldName), TextColumnDefinition);
//				sql.Append(fieldDefinition.IsNullable ? " NULL" : " NOT NULL");
//				return sql.ToString();
//			}
//
//			return base.GetColumnDefinition(
//				 fieldDefinition.FieldName,
//				 fieldDefinition.FieldType,
//				 fieldDefinition.IsPrimaryKey,
//				 fieldDefinition.AutoIncrement,
//				 fieldDefinition.IsNullable,
//				 fieldDefinition.FieldLength,
//				 null,
//				 fieldDefinition.DefaultValue);
//		}

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

		public override IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection,
			string tableName, string schemaName = null)
		{
			string sqlQuery = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @TableName";
			if (!string.IsNullOrWhiteSpace(schemaName))
			{
				sqlQuery += " AND TABLE_SCHEMA = @SchemaName";
			}

			foreach (var c in connection.Query<InformationSchema>(sqlQuery,
				new {TableName = tableName, SchemaName = schemaName}))
			{
				var ci = GetDbType(c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, c.COLUMN_TYPE);

				yield return new ColumnDefinition
				{
					Name = c.COLUMN_NAME,
					Definition = c.DATA_TYPE,
					DefaultValue = c.COLUMN_DEFAULT,
					Nullable = c.IS_NULLABLE == "YES",
					PrimaryKey = c.COLUMN_KEY == "PRI",
					Length = c.CHARACTER_MAXIMUM_LENGTH,
					DbType = c.COLUMN_TYPE == "tinyint(1)" ? DbType.Boolean : ci,
					Precision = c.NUMERIC_PRECISION,
					Scale = c.NUMERIC_SCALE
				};
			}
		}

		protected virtual DbType GetDbType(string dataType, int? length, string columnType)
        {
	        bool unsigned = columnType.ToLower().Contains("unsigned");
	        
            switch (dataType.ToUpper())
            {
                case "VARCHAR":
                case "TINYTEXT":
                case "MEDIUMTEXT":
                case "TEXT":
                case "LONGTEXT":
                    return TypesMapper.UseUnicode ? DbType.String : DbType.AnsiString;
                case "BOOLEAN":
                    return DbType.Boolean;
                case "BIT":
                case "TINYINT":
                    return unsigned ? DbType.Byte : DbType.SByte;
                case "SMALLINT":
                case "YEAR":
                    return unsigned ? DbType.UInt16 : DbType.Int16;
                case "INT":
                case "INTEGER":
                case "MEDIUMINT":
                    return unsigned ? DbType.UInt32 : DbType.Int32;
                case "BIGINT":
                    return unsigned ? DbType.UInt64 : DbType.Int64;
                case "FLOAT":
                    return DbType.Single;
                case "DOUBLE":
                    return DbType.Double;
                case "NUMERIC":
                case "DEC" :
                case "DECIMAL":
                    return DbType.Decimal;
                case "TIME":
                    return DbType.Time;
                case "DATE":
                    return DbType.Date;
                case "DATETIME":
                case "TIMESTAMP":
                    return DbType.DateTime;
                case "CHAR":
                    return length.HasValue && length == 36 ? DbType.Guid : DbType.StringFixedLength;
                case "BINARY":
                case "CHAR BYTE":
                case "VARBINARY":
                case "TINYBLOB":
                case "MEDIUMBLOB": 
                case "LONGBLOB":
                case "BLOB":
                    return DbType.Binary;
                default:
                    return DbType.Object;
            }
        }
    }
}
