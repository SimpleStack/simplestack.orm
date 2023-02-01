using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;
using MySql.Data.MySqlClient;
using SimpleStack.Orm.Expressions.Statements;

namespace SimpleStack.Orm.MySQL
{
    /// <summary>my SQL dialect provider.</summary>
    public class MySqlDialectProvider : DialectProviderBase
    {
	    /// <summary>
	    ///     Prevents a default instance of the NServiceKit.OrmLite.MySql.MySqlDialectProvider class from
	    ///     being created.
	    /// </summary>
	    public MySqlDialectProvider() : base(new MySqlDbTypeMapper())
        {
            AutoIncrementDefinition = "AUTO_INCREMENT";
            DefaultValueFormat = " DEFAULT '{0}'";
            base.SelectIdentitySql = "SELECT LAST_INSERT_ID()";
            EscapeChar = '`';
        }

        /// <summary>Creates a connection.</summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="options">         Options for controlling the operation.</param>
        /// <returns>The new connection.</returns>
        public override DbConnection CreateIDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
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
        
        public override string GetCreateSchemaStatement(string schema, bool ignoreIfExists)
        {
            return $"CREATE SCHEMA {(ignoreIfExists ? "IF NOT EXISTS" : string.Empty)} {schema}";
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

        public override IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection,
            string tableName, string schemaName = null)
        {
            var whereClause = " TABLE_NAME = @TableName";
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                whereClause += " AND TABLE_SCHEMA = @SchemaName";
            }  
            
            //Select Index with only one column to detect Unique columns
            var indexQuery = @"SELECT COLUMN_NAME, NON_UNIQUE 
                            FROM INFORMATION_SCHEMA.STATISTICS 
                            WHERE INDEX_NAME NOT IN (SELECT INDEX_NAME FROM INFORMATION_SCHEMA.STATISTICS WHERE SEQ_IN_INDEX = 2 AND "+ whereClause + ") AND "+whereClause;
            
            var columnQuery = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE " + whereClause  + " ORDER BY ordinal_position";

            var indexedColumns = connection.Query(indexQuery, new {TableName = tableName, SchemaName = schemaName})
                .ToDictionary(x => (string)x.COLUMN_NAME, x => (bool) (x.NON_UNIQUE == 0));
            

            foreach (var c in connection.Query<InformationSchema>(columnQuery,
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
                    Unique = indexedColumns.ContainsKey(c.COLUMN_NAME) && indexedColumns[c.COLUMN_NAME],
                    Length = c.CHARACTER_MAXIMUM_LENGTH,
                    DbType = c.COLUMN_TYPE == "tinyint(1)" ? DbType.Boolean : ci,
                    Precision = c.NUMERIC_PRECISION,
                    Scale = c.NUMERIC_SCALE,
                    ComputedExpression = c.GENERATION_EXPRESSION
                };
            }
        }

        protected virtual DbType GetDbType(string dataType, int? length, string columnType)
        {
            var unsigned = columnType.ToLower().Contains("unsigned");

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
                case "DEC":
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
        
        public override CommandDefinition ToInsertStatement(InsertStatement insertStatement, CommandFlags flags, CancellationToken cancellationToken = new CancellationToken())
        {
            var query = new StringBuilder("INSERT INTO ");
            query.Append(insertStatement.TableName);
            
            query.Append(" (");
            if (insertStatement.InsertFields.Any())
            {
                query.Append(insertStatement.InsertFields.Aggregate((x, y) => x + ", " + y));
            }
            query.Append(" ) VALUES (");
        
            if (insertStatement.Parameters.Any())
            {
                query.Append(insertStatement.Parameters.Select(x => x.Key).Aggregate((x, y) => x + ", " + y));
            }
            query.Append(");");
            
            query.Append(insertStatement.HasIdentity ? SelectIdentitySql : "SELECT 0");
        
            return new CommandDefinition(query.ToString(), insertStatement.GetDynamicParameters(), flags: flags, cancellationToken: cancellationToken);
        }
    }
}