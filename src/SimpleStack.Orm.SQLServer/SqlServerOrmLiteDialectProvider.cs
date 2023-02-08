using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;
using Microsoft.Data.SqlClient;
using SimpleStack.Orm.Expressions.Statements;

namespace SimpleStack.Orm.SqlServer
{
	/// <summary>A SQL server ORM lite dialect provider.</summary>
	public class SqlServerDialectProvider : DialectProviderBase
	{
		/// <summary>
		/// Initializes a new instance of the
		/// NServiceKit.OrmLite.SqlServer.SqlServerOrmLiteDialectProvider class.
		/// </summary>
		public SqlServerDialectProvider():base(new SqlServerTypeMapper())
		{
			base.AutoIncrementDefinition = "IDENTITY(1,1)";
			base.SelectIdentitySql = "SELECT SCOPE_IDENTITY()";
		}


		/// <summary>Creates a connection.</summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="options">         Options for controlling the operation.</param>
		/// <returns>The new connection.</returns>
		public override DbConnection CreateIDbConnection(string connectionString)
		{
			var isFullConnectionString = connectionString.Contains(";");

			if (!isFullConnectionString)
			{
				var filePath = connectionString;

				var filePathWithExt = filePath.ToLower().EndsWith(".mdf")
					? filePath
					: filePath + ".mdf";

				var fileName = Path.GetFileName(filePathWithExt);
				var dbName = fileName.Substring(0, fileName.Length - ".mdf".Length);

				connectionString = string.Format(
				@"Data Source=.\SQLEXPRESS;AttachDbFilename={0};Initial Catalog={1};Integrated Security=True;User Instance=True;",
					filePathWithExt, dbName);
			}

			return new SqlConnection(connectionString);
		}

		public override string GetCreateSchemaStatement(string schema, bool ignoreIfExists)
		{
			return ignoreIfExists ? 
				$@"IF NOT EXISTS ( SELECT * FROM sys.schemas WHERE name = N'{schema}' )
						EXEC('CREATE SCHEMA [{schema}]')" : 
				$"CREATE SCHEMA {schema}";
		}

		/// <summary>Gets or sets a value indicating whether this object use unicode.</summary>
		/// <value>true if use unicode, false if not.</value>


		/// <summary>Gets foreign key on delete clause.</summary>
		/// <param name="foreignKey">The foreign key.</param>
		/// <returns>The foreign key on delete clause.</returns>
		public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
		{
			return "RESTRICT" == (foreignKey.OnDelete ?? "").ToUpper()
				? ""
				: base.GetForeignKeyOnDeleteClause(foreignKey);
		}

		/// <summary>Gets foreign key on update clause.</summary>
		/// <param name="foreignKey">The foreign key.</param>
		/// <returns>The foreign key on update clause.</returns>
		public override string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
		{
			return "RESTRICT" == (foreignKey.OnDelete ?? "").ToUpper()
				? ""
				: base.GetForeignKeyOnUpdateClause(foreignKey);
		}

		/// <summary>Gets drop foreign key constraints.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The drop foreign key constraints.</returns>
		public override string GetDropForeignKeyConstraints(ModelDefinition modelDef)
		{
			//TODO: find out if this should go in base class?

			var sb = new StringBuilder();
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				if (fieldDef.ForeignKey != null)
				{
					var foreignKeyName = fieldDef.ForeignKey.GetForeignKeyName(
						modelDef,
						GetModelDefinition(fieldDef.ForeignKey.ReferenceType),
						NamingStrategy,
						fieldDef);

					var tableName = GetQuotedTableName(modelDef);
					sb.AppendLine(String.Format("IF EXISTS (SELECT name FROM sys.foreign_keys WHERE name = '{0}')", foreignKeyName));
					sb.AppendLine("BEGIN");
					sb.AppendLine(String.Format("  ALTER TABLE {0} DROP {1};", tableName, foreignKeyName));
					sb.AppendLine("END");
				}
			}

			return sb.ToString();
		}

		/// <summary>Converts this object to an add column statement.</summary>
		/// <param name="modelType">Type of the model.</param>
		/// <param name="fieldDef"> The field definition.</param>
		/// <returns>The given data converted to a string.</returns>
		public override string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
		{
			var column = GetColumnDefinition(fieldDef.FieldName,
											 fieldDef.FieldType,
											 fieldDef.IsPrimaryKey,
											 fieldDef.AutoIncrement,
											 fieldDef.IsNullable,
											 fieldDef.FieldLength,
											 fieldDef.Scale,
											 fieldDef.DefaultValue);

			return string.Format("ALTER TABLE {0} ADD {1};",
								 GetQuotedTableName(GetModel(modelType).ModelName),
								 column);
		}

		/// <summary>Converts this object to an alter column statement.</summary>
		/// <param name="modelType">Type of the model.</param>
		/// <param name="fieldDef"> The field definition.</param>
		/// <returns>The given data converted to a string.</returns>
		public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
		{
			var column = GetColumnDefinition(fieldDef.FieldName,
											 fieldDef.FieldType,
											 fieldDef.IsPrimaryKey,
											 fieldDef.AutoIncrement,
											 fieldDef.IsNullable,
											 fieldDef.FieldLength,
											 fieldDef.Scale,
											 fieldDef.DefaultValue);

			return string.Format("ALTER TABLE {0} ALTER COLUMN {1};",
								 GetQuotedTableName(GetModel(modelType).ModelName),
								 column);
		}

		public override CommandDefinition ToSelectStatement(SelectStatement statement, CommandFlags flags, CancellationToken cancellationToken = new CancellationToken())
		{
			if (statement.Offset == null && (statement.MaxRows == null || statement.MaxRows == int.MaxValue))
			{
				return base.ToSelectStatement(statement, flags, cancellationToken);
			}

			//Ensure we have an OrderBy clause, this is required by SQLServer paging (and makes more sense anyway)
			if (statement.OrderByExpression.Length == 0)
			{
				// Add Order BY 1
				statement.OrderByExpression.Append(1);
			}

			return base.ToSelectStatement(statement, flags, cancellationToken);
		}

		public override string GetLimitExpression(int? skip, int? rows)
		{
			if (!skip.HasValue && !rows.HasValue)
				return string.Empty;
			
			var sql = new StringBuilder();
			//OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY;
			sql.Append("OFFSET ");
			sql.Append(skip ?? 0);
			sql.Append(" ROWS");

			if (rows.HasValue)
			{
				sql.Append(" FETCH NEXT ");
				sql.Append(rows.Value);
				sql.Append(" ROWS ONLY");
			}

			return sql.ToString();
		}

		public override string GetDatePartFunction(string name, string quotedColName)
		{
			return $"DATEPART({name.ToLower()},{quotedColName})";
		}

		public override string GetStringFunction(string functionName, string quotedColumnName, StatementParameters parameters,
			params string[] availableParameters)
		{
			switch (functionName.ToLower())
			{
				case "length":
					return "LEN(" + quotedColumnName + ")";
				case "trim":
					return $"ltrim(rtrim({quotedColumnName}))";
				case "substring":                    
					//Ensure Offset is start at 1 instead of 0
					int offset = ((int) parameters[availableParameters[0]].Value) + 1;
					parameters[availableParameters[0]].Value = offset;

					if (parameters.Count == 2)
					{
						return $"substring({quotedColumnName},{availableParameters[0]},{availableParameters[1]})";
					}
					return $"substring({quotedColumnName},{availableParameters[0]})";
			}
			return base.GetStringFunction(functionName, quotedColumnName, parameters, availableParameters);
		}

		public override IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection, string tableName, string schemaName = null)
		{
			string fullTableName = string.IsNullOrEmpty(schemaName) ? tableName : $"{schemaName}.{tableName}";
	        
            string sqlQuery = @"SELECT c.name COLUMN_NAME,
								       t.name DATA_TYPE,
								       dc.definition COLUMN_DEFAULT,
								       c.max_length CHARACTER_MAXIMUM_LENGTH,
								       c.is_nullable IS_NULLABLE,
								       c.[precision] NUMERIC_PRECISION,
								       c.[scale] NUMERIC_SCALE,
								       cl.definition COMPUTED_DEFINITION
								FROM sys.columns c
								LEFT JOIN sys.computed_columns cl ON cl.column_id = c.column_id AND c.object_id = cl.object_id 
								LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id 
								INNER JOIN sys.types t ON t.user_type_id  = c.user_type_id 
								where c.object_id = OBJECT_ID(@TableName)
                                order by c.column_id;";

            string uniqueQuery = @"SELECT COL_NAME(ic.object_id,ic.column_id) AS COLUMN_NAME  , is_unique IS_UNIQUE, is_primary_key IS_PRIMARYKEY 
									FROM sys.indexes AS i  
									INNER JOIN sys.index_columns AS ic
									    ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
										WHERE i.object_id = OBJECT_ID(@TableName)
										AND is_unique = 1";
            
            var indexes = connection.Query(uniqueQuery, new {TableName = fullTableName}).ToArray();

            foreach (var c in connection.Query(sqlQuery, new {TableName = fullTableName}))
            {
	            var i = indexes.FirstOrDefault(x => x.COLUMN_NAME == c.COLUMN_NAME);
	            
	            yield return new ColumnDefinition
	            {
		            Name = c.COLUMN_NAME,
		            Definition = c.DATA_TYPE,
		            DefaultValue = c.COLUMN_DEFAULT,
		            PrimaryKey = i != null && i.IS_PRIMARYKEY ?? false, 
		            Unique = i != null && i.IS_UNIQUE ?? false, 
		            Length = (c.DATA_TYPE == "nvarchar" || c.DATA_TYPE == "ntext" || c.DATA_TYPE == "nchar") ? c.CHARACTER_MAXIMUM_LENGTH / 2 : c.CHARACTER_MAXIMUM_LENGTH,
		            Nullable = c.IS_NULLABLE,
		            Precision = c.NUMERIC_PRECISION,
		            Scale = c.NUMERIC_SCALE,
		            DbType = GetDbType(c.DATA_TYPE),
		            ComputedExpression = c.COMPUTED_DEFINITION
	            };
            }
        }

		protected virtual DbType GetDbType(string dataType)
		{
			switch (dataType.ToLower())
			{
				case "char":
					return DbType.AnsiStringFixedLength;
				case "varchar":
					return DbType.AnsiString;
				case "nvarchar":
					return DbType.String;
				case "nchar":
					return DbType.StringFixedLength;
				case "text":
					return DbType.AnsiString;
				case "ntext":
					return DbType.String;
				case "bit":
					return DbType.Boolean;
				case "smallint":
					return DbType.Int16;
				case "int":
					return DbType.Int32;
				case "bigint":
					return DbType.Int64;
				case "real":
					return DbType.Single;
				case "binary":
				case "varbinary":
					return DbType.Binary;
				case "numeric":
				case "decimal":
					return DbType.Decimal;
				case "date":
					return DbType.Date;
				case "time":
					return DbType.Time;
				case "timestamp":
				case "datetime":
				case "smalldatetime":
					return DbType.DateTime;
				case "datetime2":
					return DbType.DateTime2;
				case "datetimeoffset":
					return DbType.DateTimeOffset;	
				case "uniqueidentifier":
					return DbType.Guid;
				case "money":
				case "smallmoney":
					return DbType.Currency;	
				case "xml":
					return DbType.Xml;
				default:
					return DbType.Object;
			}
		}
	}
}
