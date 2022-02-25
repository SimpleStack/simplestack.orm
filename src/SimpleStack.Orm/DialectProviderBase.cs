//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions.Statements;
using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
    /// <summary>An ORM lite dialect provider base.</summary>
    public abstract class DialectProviderBase : IDialectProvider
    {
        /// <summary>The AutoIncrement column definition.</summary>
        protected string AutoIncrementDefinition = "AUTOINCREMENT";

        /// <summary>The default value format.</summary>
        protected string DefaultValueFormat = " DEFAULT ('{0}')";

        protected char EscapeChar = '\"';

        /// <summary>
        ///     Initializes a new instance of the NServiceKit.OrmLite.OrmLiteDialectProviderBase&lt;
        ///     TDialect&gt; class.
        /// </summary>
        protected DialectProviderBase(IDbTypeMapper mapper)
        {
            TypesMapper = mapper;
            NamingStrategy = new NamingStrategyBase();
            ParamPrefix = "@";
        }


        /// <summary>Gets or sets the select identity SQL.</summary>
        /// <value>The select identity SQL.</value>
        public virtual string SelectIdentitySql { get; set; }


        /// <summary>Gets or sets the parameter string.</summary>
        /// <value>The parameter string.</value>
        public string ParamPrefix { get; set; }

        /// <summary>Gets or sets the naming strategy.</summary>
        /// <value>The naming strategy.</value>
        public INamingStrategy NamingStrategy { get; set; }

        public IDbTypeMapper TypesMapper { get; set; }

        /// <summary>Creates a connection.</summary>
        /// <param name="connectionString">Connection String.</param>
        /// <returns>The new connection.</returns>
        public OrmConnection CreateConnection(string connectionString, ILoggerFactory logger)
        {
            return new OrmConnection(CreateIDbConnection(connectionString), this, logger);
        }

        public string GetParameterName(int parameterCount)
        {
            return $"{ParamPrefix}p_{parameterCount}";
        }

        /// <summary>Gets quoted table name.</summary>
        /// <param name="modelDef">The model definition.</param>
        /// <returns>The quoted table name.</returns>
        public string GetQuotedTableName(ModelDefinition modelDef)
        {
            return GetQuotedTableName(modelDef.Alias ?? modelDef.ModelName, modelDef.Schema);
        }

        /// <summary>Gets quoted table name.</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema (optional)</param>
        /// <returns>The quoted table name.</returns>
        public virtual string GetQuotedTableName(string tableName, string schemaName = null)
        {
            return string.IsNullOrEmpty(schemaName)
                ? $"{EscapeChar}{NamingStrategy.GetTableName(tableName)}{EscapeChar}"
                : $"{EscapeChar}{NamingStrategy.GetTableName(schemaName)}{EscapeChar}.{EscapeChar}{NamingStrategy.GetTableName(tableName)}{EscapeChar}";
        }

        /// <summary>Gets quoted column name.</summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The quoted column name.</returns>
        public virtual string GetQuotedColumnName(string columnName)
        {
            return $"{EscapeChar}{NamingStrategy.GetColumnName(columnName)}{EscapeChar}";
        }

        /// <inheritdoc />
        public virtual CommandDefinition ToSelectStatement(SelectStatement statement, CommandFlags flags)
        {
            var sql = new StringBuilder("SELECT ");

            if (statement.IsDistinct)
            {
                sql.Append(" DISTINCT ");
            }

            sql.Append(statement.Columns.Count == 0 ? "*" : statement.Columns.Aggregate((x, y) => x + ", " + y));

            sql.Append("\n FROM ");
            sql.Append(statement.TableName);

            if (statement.WhereExpression.Length > 0)
            {
                sql.Append("\n WHERE ");
                sql.Append(statement.WhereExpression);
            }

            if (statement.GroupByExpression.Length > 0)
            {
                sql.Append("\n GROUP BY ");
                sql.Append(statement.GroupByExpression);
            }

            if (statement.HavingExpression.Length > 0)
            {
                sql.Append("\n HAVING ");
                sql.Append(statement.HavingExpression);
            }

            if (statement.OrderByExpression.Length > 0)
            {
                sql.Append("\n ORDER BY ");
                sql.Append(statement.OrderByExpression);
            }

            var limit = GetLimitExpression(statement.Offset, statement.MaxRows);
            if (limit.Length > 0)
            {
                sql.Append("\n ");
                sql.Append(limit);
            }


            return new CommandDefinition(sql.ToString(), statement.Parameters, flags: flags);
        }

        public virtual CommandDefinition ToCountStatement(CountStatement statement, CommandFlags flags)
        {
            var sql = new StringBuilder("SELECT COUNT(");

            if (statement.IsDistinct)
            {
                sql.Append(" DISTINCT ");
            }

            sql.Append(statement.Columns.Count == 1 ? statement.Columns[0] : "*");
            sql.Append(")\n FROM ");
            sql.Append(statement.TableName);

            if (statement.WhereExpression.Length > 0)
            {
                sql.Append("\n WHERE ");
                sql.Append(statement.WhereExpression);
            }

            if (statement.GroupByExpression.Length > 0)
            {
                sql.Append("\n GROUP BY ");
                sql.Append(statement.GroupByExpression);
            }

            if (statement.HavingExpression.Length > 0)
            {
                sql.Append("\n HAVING ");
                sql.Append(statement.HavingExpression);
            }

            return new CommandDefinition(sql.ToString(), statement.Parameters, flags: flags);
        }

        public virtual CommandDefinition ToDeleteStatement(DeleteStatement statement)
        {
            var query = new StringBuilder("DELETE FROM ");
            query.Append(statement.TableName);
            if (statement.WhereExpression.Length > 0)
            {
                query.Append(" WHERE ");
                query.Append(statement.WhereExpression);
            }

            return new CommandDefinition(query.ToString(), statement.Parameters);
        }

        public virtual CommandDefinition ToInsertStatement(InsertStatement insertStatement, CommandFlags flags)
        {
            var query = new StringBuilder("INSERT INTO ");
            query.Append(insertStatement.TableName);
            if (insertStatement.InsertFields.Any())
            {
                query.Append(" (");
                query.Append(insertStatement.InsertFields.Aggregate((x, y) => x + ", " + y));
                query.Append(" ) VALUES (");
                query.Append(insertStatement.Parameters.Select(x => x.Key).Aggregate((x, y) => x + ", " + y));
                query.Append(");");
            }
            else
            {
                query.Append(" DEFAULT VALUES; ");
            }

            query.Append(insertStatement.HasIdentity ? SelectIdentitySql : "SELECT 0");

            return new CommandDefinition(query.ToString(), insertStatement.Parameters, flags: flags);
        }


        public virtual CommandDefinition ToUpdateStatement(UpdateStatement statement, CommandFlags flags)
        {
            var query = new StringBuilder("UPDATE ");
            query.Append(statement.TableName);
            query.Append(" SET ");

            var first = true;
            foreach (var f in statement.UpdateFields)
            {
                if (!first)
                {
                    query.Append(", ");
                }

                query.Append(f.Key);
                query.Append("=");
                query.Append(f.Value);
                first = false;
            }

            if (statement.WhereExpression.Length > 0)
            {
                query.Append(" WHERE ");
                query.Append(statement.WhereExpression);
            }

            return new CommandDefinition(query.ToString(), statement.Parameters, flags: flags);
        }

        /// <summary>Converts a tableType to a create table statement.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>tableType as a string.</returns>
        public virtual string ToCreateTableStatement(ModelDefinition modelDef)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();
            var sbPrimaryKeys = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions.Where(x => !x.IsComputed))
            {
                if (sbColumns.Length != 0)
                {
                    sbColumns.Append(", \n  ");
                }

                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.FieldType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
                    null,
                    fieldDef.DefaultValue);

                sbColumns.Append(columnDefinition);

                if (fieldDef.IsPrimaryKey && !fieldDef.AutoIncrement)
                {
                    sbPrimaryKeys.Append(sbPrimaryKeys.Length == 0 ? ", PRIMARY KEY(" : ",");
                    sbPrimaryKeys.Append(GetQuotedColumnName(fieldDef.FieldName));
                }

                if (fieldDef.ForeignKey != null)
                {
                    var refModelDef = fieldDef.ForeignKey.ReferenceType.GetModelDefinition();
                    sbConstraints.AppendFormat(
                        ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                        GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy,
                            fieldDef)),
                        GetQuotedColumnName(fieldDef.FieldName),
                        GetQuotedTableName(refModelDef),
                        GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                    sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                    sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
                }
            }

            if (sbPrimaryKeys.Length > 0)
            {
                sbPrimaryKeys.Append(")");
            }

            return
                $"CREATE TABLE {GetQuotedTableName(modelDef)} \n(\n  {sbColumns}{sbPrimaryKeys}{sbConstraints} \n); \n";
        }

        /// <summary>Converts a tableType to a create index statements.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>tableType as a List&lt;string&gt;</returns>
        public virtual List<string> ToCreateIndexStatements(ModelDefinition modelDef)
        {
            var sqlIndexes = new List<string>();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed)
                {
                    continue;
                }

                var indexName = GetIndexName(fieldDef.IsUnique, modelDef.ModelName.SafeVarName(), fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexName(compositeIndex, modelDef);
                var indexNames = string.Join(" ASC, ",
                    compositeIndex.FieldNames.Select(GetQuotedColumnName).ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, true));
            }

            return sqlIndexes;
        }

        /// <summary>Query if 'dbCmd' does table exist.</summary>
        /// <param name="connection">The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool DoesTableExist(IDbConnection connection, string tableName, string schemaName = null)
        {
            return false;
        }

        /// <summary>Query if 'dbCmd' does table exist.</summary>
        /// <param name="connection">The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool DoesSchemaExist(IDbConnection connection, string schemaName)
        {
            return false;
        }

        /// <summary>Query if 'dbCmd' does sequence exist.</summary>
        /// <param name="dbCmd">       The database command.</param>
        /// <param name="sequenceName">Name of the sequenc.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool DoesSequenceExist(IDbConnection dbCmd, string sequenceName)
        {
            return true;
        }

        /// <summary>Gets column names.</summary>
        /// <param name="modelDef">The model definition.</param>
        /// <returns>The column names.</returns>
        public virtual IEnumerable<string> GetColumnNames(ModelDefinition modelDef)
        {
            foreach (var fd in modelDef.FieldDefinitions)
            {
                if (fd.IsComputed)
                {
                    yield return $"{fd.ComputeExpression} AS {GetQuotedColumnName(fd.Name)}";
                }
                else if (fd.FieldName != fd.Name)
                {
                    yield return $"{GetQuotedColumnName(fd.FieldName)} AS {GetQuotedColumnName(fd.Name)}";
                }
                else
                {
                    yield return GetQuotedColumnName(fd.FieldName);
                }
            }
        }

        /// <summary>Converts a tableType to a create sequence statements.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>tableType as a List&lt;string&gt;</returns>
        public virtual List<string> ToCreateSequenceStatements(ModelDefinition modelDef)
        {
            return new List<string>();
        }

        /// <summary>Converts this object to a create sequence statement.</summary>
        /// <param name="modelDef">Model Definition</param>
        /// <param name="sequenceName">Name of the sequence.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToCreateSequenceStatement(ModelDefinition modelDef, string sequenceName)
        {
            return string.Empty;
        }


        public virtual string GetCreateSchemaStatement(string schema, bool ignoreIfExists)
        {
            return "SELECT 1";
        }

        /// <summary>Sequence list.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>A List&lt;string&gt;</returns>
        public virtual List<string> SequenceList(ModelDefinition modelDef)
        {
            return new List<string>();
        }

        /// <summary>Gets drop foreign key constraints.</summary>
        /// <param name="modelDef">The model definition.</param>
        /// <returns>The drop foreign key constraints.</returns>
        public virtual string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            return null;
        }

        /// <summary>Gets index name.</summary>
        /// <param name="isUnique"> true if this object is unique.</param>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The index name.</returns>
        public virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return $"{(isUnique ? "u" : "")}idx_{modelName}_{fieldName}".ToLower();
        }

        public virtual string GetDatePartFunction(string name, string quotedColName)
        {
            throw new NotImplementedException();
        }

        public virtual string GetDropTableStatement(ModelDefinition modelDef)
        {
            return "DROP TABLE " + GetQuotedTableName(modelDef);
        }

        public virtual IEnumerable<IColumnDefinition> GetTableColumnDefinitions(
            IDbConnection connection,
            string tableName,
            string schemaName = null)
        {
            return new IColumnDefinition[0];
        }

        public virtual async Task<IEnumerable<ITableDefinition>> GetTableDefinitions(
            IDbConnection connection,
            string schemaName = null,
            bool includeViews = false)
        {
            var sqlQuery = "SELECT table_name,table_schema FROM INFORMATION_SCHEMA.TABLES where";

            if (includeViews)
            {
                sqlQuery += " (TABLE_TYPE = 'BASE TABLE' OR TABLE_TYPE = 'VIEW')";
            }
            else
            {
                sqlQuery += " TABLE_TYPE = 'BASE TABLE'";
            }

            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                sqlQuery +=
                    $" AND {GetStringFunction("ToLower", "table_schema", null)} = {GetStringFunction("ToLower", "@SchemaName", null)} ";
            }

            var tables = new List<TableDefinition>();
            foreach (var table in await connection.QueryAsync(sqlQuery, new {SchemaName = schemaName}).ConfigureAwait(false))
            {
                tables.Add(new TableDefinition
                {
                    Name = table.table_name,
                    SchemaName = table.table_schema
                });
            }

            return tables;
        }

        public virtual string BindOperand(ExpressionType e, bool isIntegral)
        {
            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                case ExpressionType.And:
                    return isIntegral ? "&" : "AND";
                case ExpressionType.Or:
                    return isIntegral ? "|" : "OR";
                case ExpressionType.ExclusiveOr:
                    return isIntegral ? "^" : "XOR";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return null;
            }
        }

        public virtual string GetStringFunction(string functionName, string quotedColumnName,
            IDictionary<string, object> parameters, params string[] availableParameters)
        {
            switch (functionName.ToLower())
            {
                case "length":
                    return "LENGTH(" + quotedColumnName + ")";
                case "trim":
                    return $"trim({quotedColumnName})";
                case "trimstart":
                    return $"ltrim({quotedColumnName})";
                case "trimend":
                    return $"rtrim({quotedColumnName})";
                case "toupper":
                    return $"upper({quotedColumnName})";
                case "tolower":
                    return $"lower({quotedColumnName})";
                case "startswith":
                    parameters[availableParameters[0]] = parameters[availableParameters[0]].ToString().ToUpper() + "%";
                    return $"upper({quotedColumnName}) LIKE {availableParameters[0]} ";
                case "endswith":
                    parameters[availableParameters[0]] = "%" + parameters[availableParameters[0]].ToString().ToUpper();
                    return $"upper({quotedColumnName}) LIKE {availableParameters[0]} ";
                case "contains":
                    parameters[availableParameters[0]] =
                        "%" + parameters[availableParameters[0]].ToString().ToUpper() + "%";
                    return $"upper({quotedColumnName}) LIKE {availableParameters[0]} ";
                case "substring":
                    //Ensure Offset is start at 1 instead of 0
                    var offset = (int) parameters[availableParameters[0]] + 1;
                    parameters[availableParameters[0]] = offset;

                    if (parameters.Count == 2)
                    {
                        return $"substr({quotedColumnName},{availableParameters[0]},{availableParameters[1]})";
                    }

                    return $"substr({quotedColumnName},{availableParameters[0]})";
                default:
                    throw new NotSupportedException();
            }
        }

        public abstract DbConnection CreateIDbConnection(string connectionString);

        /// <summary>Gets quoted name.</summary>
        /// <param name="name">Name of the column.</param>
        /// <returns>The quoted name.</returns>
        public virtual string GetQuotedName(string name)
        {
            return $"{EscapeChar}{name}{EscapeChar}";
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
        public virtual string GetColumnDefinition(string fieldName, Type fieldType,
            bool isPrimaryKey, bool autoIncrement, bool isNullable,
            int? fieldLength, int? scale, object defaultValue)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName),
                GetColumnTypeDefinition(fieldType, fieldName, fieldLength));

            if (isPrimaryKey)
            {
                if (autoIncrement)
                {
                    sql.Append(" PRIMARY KEY");
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                sql.Append(isNullable ? " NULL" : " NOT NULL");
            }

            if (defaultValue != null)
            {
                sql.AppendFormat(DefaultValueFormat, GetDefaultValueDefinition(defaultValue));
            }

            return sql.ToString();
        }

        protected virtual string GetDefaultValueDefinition(object defaultValue)
        {
            return defaultValue.ToString();
        }

        /// <summary>Gets column database type.</summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>The column database type.</returns>
        /// <summary>Gets column type definition.</summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="fieldName"></param>
        /// <param name="fieldLength"></param>
        /// <returns>The column type definition.</returns>
        public virtual string GetColumnTypeDefinition(Type fieldType, string fieldName, int? fieldLength)
        {
#pragma warning disable 618
            var dbType = SqlMapper.LookupDbType(fieldType, fieldName, false, out var typeHandler);
#pragma warning restore 618

            if (typeHandler is ITypeHandlerColumnType typeHandlerColumnType)
            {
                dbType = typeHandlerColumnType.ColumnType;
                fieldLength = typeHandlerColumnType.Length;
            }

            return TypesMapper.GetFieldDefinition(dbType, fieldLength);
        }

        /// <summary>Gets foreign key on delete clause.</summary>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns>The foreign key on delete clause.</returns>
        public virtual string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnDelete) ? " ON DELETE " + foreignKey.OnDelete : "";
        }

        /// <summary>Gets foreign key on update clause.</summary>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns>The foreign key on update clause.</returns>
        public virtual string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnUpdate) ? " ON UPDATE " + foreignKey.OnUpdate : "";
        }

        /// <summary>Gets composite index name.</summary>
        /// <param name="compositeIndex">Zero-based index of the composite.</param>
        /// <param name="modelDef">      The model definition.</param>
        /// <returns>The composite index name.</returns>
        protected virtual string GetCompositeIndexName(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        /// <summary>Converts this object to a create index statement.</summary>
        /// <param name="isUnique">  true if this object is unique.</param>
        /// <param name="indexName"> Name of the index.</param>
        /// <param name="modelDef">  The model definition.</param>
        /// <param name="fieldName"> Name of the field.</param>
        /// <param name="isCombined">true if this object is combined.</param>
        /// <returns>The given data converted to a string.</returns>
        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef,
            string fieldName, bool isCombined = false)
        {
            return
                $"CREATE {(isUnique ? "UNIQUE" : string.Empty)} INDEX {indexName} ON {GetQuotedTableName(modelDef)} ({(isCombined ? fieldName : GetQuotedColumnName(fieldName))} ASC); \n";
        }

        /// <summary>Gets a model.</summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>The model.</returns>
        protected static ModelDefinition GetModel(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        /// <summary>Gets model definition.</summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>The model definition.</returns>
        public static ModelDefinition GetModelDefinition(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        #region DDL

        /// <summary>Converts this object to an add column statement.</summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="fieldDef"> The field definition.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef.FieldName,
                fieldDef.FieldType,
                fieldDef.IsPrimaryKey,
                fieldDef.AutoIncrement,
                fieldDef.IsNullable,
                fieldDef.FieldLength,
                fieldDef.Scale,
                fieldDef.DefaultValue);
            return $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} ADD COLUMN {column};";
        }

        /// <summary>Converts this object to an alter column statement.</summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="fieldDef"> The field definition.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef.FieldName,
                fieldDef.FieldType,
                fieldDef.IsPrimaryKey,
                fieldDef.AutoIncrement,
                fieldDef.IsNullable,
                fieldDef.FieldLength,
                fieldDef.Scale,
                fieldDef.DefaultValue);
            return
                $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} MODIFY COLUMN {column};";
        }

        /// <summary>Converts this object to a change column name statement.</summary>
        /// <param name="modelType">    Type of the model.</param>
        /// <param name="fieldDef">     The field definition.</param>
        /// <param name="oldColumnName">Name of the old column.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToChangeColumnNameStatement(
            Type modelType,
            FieldDefinition fieldDef,
            string oldColumnName)
        {
            var column = GetColumnDefinition(fieldDef.FieldName,
                fieldDef.FieldType,
                fieldDef.IsPrimaryKey,
                fieldDef.AutoIncrement,
                fieldDef.IsNullable,
                fieldDef.FieldLength,
                fieldDef.Scale,
                fieldDef.DefaultValue);
            return
                $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} CHANGE COLUMN {GetQuotedColumnName(oldColumnName)} {column};";
        }

        /// <summary>Converts this object to an add foreign key statement.</summary>
        /// <typeparam name="T">       Generic type parameter.</typeparam>
        /// <typeparam name="TForeign">Type of the foreign.</typeparam>
        /// <param name="field">         The field.</param>
        /// <param name="foreignField">  The foreign field.</param>
        /// <param name="onUpdate">      The on update.</param>
        /// <param name="onDelete">      The on delete.</param>
        /// <param name="foreignKeyName">Name of the foreign key.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
            Expression<Func<TForeign, object>> foreignField,
            OnFkOption onUpdate,
            OnFkOption onDelete,
            string foreignKeyName = null)
        {
            var sourceMd = ModelDefinition<T>.Definition;
            var fieldName = sourceMd.GetFieldDefinition(field).FieldName;

            var referenceMd = ModelDefinition<TForeign>.Definition;
            var referenceFieldName = referenceMd.GetFieldDefinition(foreignField).FieldName;

            var name = GetQuotedName(string.IsNullOrEmpty(foreignKeyName)
                ? "fk_" + sourceMd.ModelName + "_" + fieldName + "_" + referenceFieldName
                : foreignKeyName);

            return $"ALTER TABLE {GetQuotedTableName(sourceMd.ModelName)} " +
                   $"ADD CONSTRAINT {name} FOREIGN KEY ({GetQuotedColumnName(fieldName)}) " +
                   $"REFERENCES {GetQuotedTableName(referenceMd.ModelName)} ({GetQuotedColumnName(referenceFieldName)})" +
                   $"{GetForeignKeyOnDeleteClause(new ForeignKeyConstraint(typeof(T), FkOptionToString(onDelete)))}" +
                   $"{GetForeignKeyOnUpdateClause(new ForeignKeyConstraint(typeof(T), onUpdate: FkOptionToString(onUpdate)))};";
        }

        /// <summary>Converts this object to a create index statement.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="field">    The field.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="unique">   true to unique.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual string ToCreateIndexStatement<T>(Expression<Func<T, object>> field,
            string indexName = null, bool unique = false)
        {
            var sourceMd = ModelDefinition<T>.Definition;
            var fieldName = sourceMd.GetFieldDefinition(field).FieldName;

            var name = GetQuotedName(string.IsNullOrWhiteSpace(indexName)
                ? (unique ? "uidx" : "idx") + "_" + sourceMd.ModelName + "_" + fieldName
                : indexName);

            var command = $"CREATE{(unique ? " UNIQUE " : " ")}INDEX {name} " +
                          $"ON {GetQuotedTableName(sourceMd.ModelName)}({GetQuotedColumnName(fieldName)});";
            return command;
        }

        public virtual string GetLimitExpression(int? skip, int? rows)
        {
            if (!skip.HasValue && !rows.HasValue)
            {
                return string.Empty;
            }

            //LIMIT 10 OFFSET 15
            var limit = new StringBuilder();
            if (rows.HasValue)
            {
                limit.Append(" LIMIT ");
                limit.Append(rows.Value);
            }

            if (skip.HasValue)
            {
                limit.Append(" OFFSET ");
                limit.Append(skip.Value);
            }

            return limit.ToString();
        }

        /// <summary>Fk option to string.</summary>
        /// <param name="option">The option.</param>
        /// <returns>A string.</returns>
        protected virtual string FkOptionToString(OnFkOption option)
        {
            switch (option)
            {
                case OnFkOption.Cascade:
                    return "CASCADE";
                case OnFkOption.NoAction:
                    return "NO ACTION";
                case OnFkOption.SetNull:
                    return "SET NULL";
                case OnFkOption.SetDefault:
                    return "SET DEFAULT";
                case OnFkOption.Restrict:
                default:
                    return "RESTRICT";
            }
        }

        #endregion DDL
    }
}