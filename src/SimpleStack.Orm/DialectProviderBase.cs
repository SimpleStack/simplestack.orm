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
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
    /// <summary>An ORM lite dialect provider base.</summary>
    public abstract class DialectProviderBase : IDialectProvider
    {
        /// <summary>The AutoIncrement column definition.</summary>
        public string AutoIncrementDefinition = "AUTOINCREMENT";

        /// <summary>The BLOB column definition.</summary>
        public string BlobColumnDefinition = "BLOB";

        /// <summary>The column definition.</summary>
        public string BoolColumnDefinition = "BOOL";

        /// <summary>The date time column definition.</summary>
        public string DateTimeColumnDefinition = "DATETIME";

        /// <summary>The database type map.</summary>
        protected DbTypes DbTypeMap = new DbTypes();

        /// <summary>The decimal column definition.</summary>
        public string DecimalColumnDefinition = "DECIMAL";

        /// <summary>SqlServer express limit.</summary>
        private int _defaultStringLength = 8000;

        /// <summary>The default value format.</summary>
        public string DefaultValueFormat = " DEFAULT ({0})";

        /// <summary>The unique identifier column definition.</summary>
        public string GuidColumnDefinition = "GUID";

        /// <summary>The int column definition.</summary>
        public string IntColumnDefinition = "INTEGER";

        /// <summary>The long column definition.</summary>
        public string LongColumnDefinition = "BIGINT";

        /// <summary>The real column definition.</summary>
        public string RealColumnDefinition = "DOUBLE";

        /// <summary>Set by Constructor and UpdateStringColumnDefinitions()</summary>
        public string StringColumnDefinition;

        /// <summary>The string length column definition format.</summary>
        public string StringLengthColumnDefinitionFormat;

        /// <summary>The string length non unicode column definition format.</summary>
        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";

        /// <summary>The string length unicode column definition format.</summary>
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        /// <summary>The time column definition.</summary>
        public string TimeColumnDefinition = "DATETIME";

        /// <summary>true to use unicode.</summary>
        protected bool useUnicode;

        /// <summary>
        ///     Initializes a new instance of the NServiceKit.OrmLite.OrmLiteDialectProviderBase&lt;
        ///     TDialect&gt; class.
        /// </summary>
        protected DialectProviderBase()
        {
            NamingStrategy = new NamingStrategyBase();
            DefaultDecimalScale = 12;
            DefaultDecimalPrecision = 18;
            ParamPrefix = "@";
            UpdateStringColumnDefinitions();
        }

        /// <summary>Gets or sets the default decimal precision.</summary>
        /// <value>The default decimal precision.</value>
        public int DefaultDecimalPrecision { get; set; }

        /// <summary>Gets or sets the default decimal scale.</summary>
        /// <value>The default decimal scale.</value>
        public int DefaultDecimalScale { get; set; }

        /// <summary>Gets or sets the select identity SQL.</summary>
        /// <value>The select identity SQL.</value>
        public virtual string SelectIdentitySql { get; set; }

        /// <summary>Gets or sets the default string length.</summary>
        /// <value>The default string length.</value>
        public int DefaultStringLength
        {
            get { return _defaultStringLength; }
            set
            {
                _defaultStringLength = value;
                UpdateStringColumnDefinitions();
            }
        }

        /// <summary>Gets or sets the parameter string.</summary>
        /// <value>The parameter string.</value>
        public string ParamPrefix { get; set; }

        /// <summary>Gets or sets a value indicating whether this object use unicode.</summary>
        /// <value>true if use unicode, false if not.</value>
        public virtual bool UseUnicode
        {
            get { return useUnicode; }
            set
            {
                useUnicode = value;
                UpdateStringColumnDefinitions();
            }
        }

        /// <summary>Gets or sets the naming strategy.</summary>
        /// <value>The naming strategy.</value>
        public INamingStrategy NamingStrategy { get; set; }

        /// <summary>Creates a connection.</summary>
        /// <param name="connectionString">Connection String.</param>
        /// <returns>The new connection.</returns>
        public OrmConnection CreateConnection(string connectionString)
        {
            return new OrmConnection(CreateIDbConnection(connectionString), this);
        }

        public abstract DbConnection CreateIDbConnection(string connectionString);

        public string GetParameterName(int parameterCount)
        {
            return $"{ParamPrefix}p_{parameterCount}";
        }

        /// <summary>Gets quoted table name.</summary>
        /// <param name="modelDef">The model definition.</param>
        /// <returns>The quoted table name.</returns>
        public virtual string GetQuotedTableName(ModelDefinition modelDef)
        {
            return GetQuotedTableName(modelDef.ModelName);
        }

        /// <summary>Gets quoted table name.</summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The quoted table name.</returns>
        public virtual string GetQuotedTableName(string tableName)
        {
            return $"\"{NamingStrategy.GetTableName(tableName)}\"";
        }

        /// <summary>Gets quoted column name.</summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The quoted column name.</returns>
        public virtual string GetQuotedColumnName(string columnName)
        {
            return $"\"{NamingStrategy.GetColumnName(columnName)}\"";
        }

        /// <summary>Gets quoted name.</summary>
        /// <param name="name">Name of the column.</param>
        /// <returns>The quoted name.</returns>
        public virtual string GetQuotedName(string name)
        {
            return $"\"{name}\"";
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
            int? fieldLength, int? scale, string defaultValue)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), GetColumnTypeDefinition(fieldType, fieldName, fieldLength));

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

            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return sql.ToString();
        }

        public virtual CommandDefinition ToCountStatement<T>(SqlExpressionVisitor<T> visitor)
        {
            var modelDef = typeof(T).GetModelDefinition();
            var sql = new StringBuilder();

            sql.AppendFormat("SELECT COUNT({0}) FROM {1}",
                visitor.Fields.Count == 0 ? "*" : (visitor.IsDistinct ? "DISTINCT" : String.Empty) + visitor.Fields.Aggregate((x, y) => x + ", " + y),
                GetQuotedTableName(modelDef));

            if (!string.IsNullOrEmpty(visitor.WhereExpression))
            {
                sql.Append(visitor.WhereExpression);
            }

            if (!string.IsNullOrEmpty(visitor.LimitExpression))
            {
                sql.Append(" ");
                sql.Append(visitor.LimitExpression);
            }
            return new CommandDefinition(sql.ToString(), visitor.Parameters);
        }

        public virtual CommandDefinition ToSelectStatement<T>(SqlExpressionVisitor<T> visitor, CommandFlags flags)
        {
            var sql = new StringBuilder();

            sql.Append(visitor.SelectExpression);

            sql.Append(string.IsNullOrEmpty(visitor.WhereExpression)
                ? String.Empty
                : "\n" + visitor.WhereExpression);
            sql.Append(string.IsNullOrEmpty(visitor.GroupByExpression)
                ? String.Empty
                : "\n" + visitor.GroupByExpression);
            sql.Append(string.IsNullOrEmpty(visitor.HavingExpression)
                ? String.Empty
                : "\n" + visitor.HavingExpression);
            sql.Append(string.IsNullOrEmpty(visitor.OrderByExpression)
                ? String.Empty
                : "\n" + visitor.OrderByExpression);
            sql.Append(string.IsNullOrEmpty(visitor.LimitExpression)
                ? String.Empty
                : "\n" + visitor.LimitExpression);

            return new CommandDefinition(sql.ToString(), visitor.Parameters, flags: flags);
        }

        public virtual CommandDefinition ToInsertRowStatement<T>(T objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();

            ModelDefinition modelDef = objWithProperties.GetType().GetModelDefinition();
            List<FieldDefinition> fieldDefs = modelDef.FieldDefinitions
                .Where(fieldDef => !fieldDef.IsComputed)
                .Where(fieldDef => !fieldDef.AutoIncrement)
                .Where(fieldDef => insertFields.Count <= 0 || insertFields.Contains(fieldDef.Name)).ToList();

            var parameters = new Dictionary<string, object>();

            sbColumnValues.Append('(');

            var isFirstField = true;
            foreach (var fieldDef in fieldDefs)
            {
                if (!isFirstField)
                {
                    sbColumnNames.Append(',');
                }
                sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));


                if (!isFirstField)
                {
                    sbColumnValues.Append(',');
                }

                string paramName = GetParameterName(parameters.Count);
                sbColumnValues.Append(paramName);
                parameters.Add(paramName, fieldDef.GetValue(objWithProperties));
                isFirstField = false;
            }
            sbColumnValues.Append(')');

            var sql = $"INSERT INTO {GetQuotedTableName(modelDef)} ({sbColumnNames}) VALUES {sbColumnValues}";

            return new CommandDefinition(sql, parameters);
        }

        /// <summary>Converts this object to an update row statement.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="objWithProperties">The object with properties.</param>
        /// <param name="ev">The expression visitor.</param>
        /// <returns>The given data converted to a string.</returns>
        public virtual CommandDefinition ToUpdateRowStatement<T>(T objWithProperties, SqlExpressionVisitor<T> ev)
        {
            var modelDef = objWithProperties.GetType().GetModelDefinition();
            var parameters = new Dictionary<string, object>();

            var setSql = new StringBuilder();
            var whereSql = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsPrimaryKey)
                {
                    whereSql.Append(whereSql.Length == 0 ? "WHERE " : " AND ");

                    var parameterName = GetParameterName(parameters.Count);
                    whereSql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), parameterName);
                    parameters.Add(parameterName, fieldDef.GetValueFn(objWithProperties));
                }
                else if (ev.Fields.Count == 0 || ev.Fields.Contains(fieldDef.Name))
                {
                    if (setSql.Length > 0)
                    {
                        setSql.Append(",");
                    }
                    var paramName = GetParameterName(parameters.Count);
                    setSql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), paramName);
                    parameters.Add(paramName, fieldDef.GetValue(objWithProperties));
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}", GetQuotedTableName(modelDef), setSql, whereSql);

            if (setSql.Length == 0)
                throw new OrmException("No valid update properties provided (e.g. p => p.FirstName): " + updateSql);

            return new CommandDefinition(updateSql, parameters);
        }

        public virtual CommandDefinition ToUpdateAllRowStatement<T>(object updateOnly, SqlExpressionVisitor<T> ev)
        {
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            var parameters = new Dictionary<string, object>(ev.Parameters);

            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef =
                    fields.FirstOrDefault(x => x.Name.EqualsIgnoreCase(setField.Name));

                if (fieldDef == null || fieldDef.IsComputed || fieldDef.IsPrimaryKey)
                    continue;

                if (ev.Fields.Count == 0 || ev.Fields.Contains(fieldDef.Name))
                {
                    if (sql.Length > 0)
                    {
                        sql.Append(",");
                    }

                    var parameterName = GetParameterName(parameters.Count);

                    sql.AppendFormat("{0} = {1}",
                        GetQuotedColumnName(fieldDef.FieldName),
                        parameterName);

                    parameters.Add(parameterName, setField.GetPropertyGetterFn()(updateOnly));
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}",
                GetQuotedTableName(modelDef), sql, ev.WhereExpression);

            return new CommandDefinition(updateSql, parameters);
        }

        public virtual CommandDefinition ToDeleteRowStatement<T>(SqlExpressionVisitor<T> visitor)
        {
            return new CommandDefinition($"DELETE FROM {GetQuotedTableName(visitor.ModelDefinition)} {visitor.WhereExpression}", visitor.Parameters);
        }

        public virtual CommandDefinition ToDeleteRowStatement<T>(T objWithProperties)
        {
            var whereSql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            var parameters = new Dictionary<string, object>();

            bool hasPrimaryKey = fields.Any(x => x.IsPrimaryKey);

            var queryFields = hasPrimaryKey ? fields.Where(x => x.IsPrimaryKey) : fields;

            foreach (var fieldDef in queryFields)
            {
                if (whereSql.Length > 0)
                {
                    whereSql.Append(" AND ");
                }
                var parameterName = GetParameterName(parameters.Count);
                whereSql.Append($"{GetQuotedColumnName(fieldDef.FieldName)} = {parameterName}");
                parameters.Add(parameterName, fieldDef.GetValueFn(objWithProperties));
            }

            return new CommandDefinition($"DELETE FROM {GetQuotedTableName(modelDef)} WHERE {whereSql}", parameters);
        }

        /// <summary>Converts a tableType to a create table statement.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>tableType as a string.</returns>
        public virtual string ToCreateTableStatement(ModelDefinition modelDef)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();
            var sbPrimaryKeys = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

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
                        GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
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

            return $"CREATE TABLE {GetQuotedTableName(modelDef)} \n(\n  {sbColumns}{sbPrimaryKeys}{sbConstraints} \n); \n";
        }

        /// <summary>Converts a tableType to a create index statements.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>tableType as a List&lt;string&gt;</returns>
        public virtual List<string> ToCreateIndexStatements(ModelDefinition modelDef)
        {
            var sqlIndexes = new List<string>();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

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
            string fieldDefinition;
#pragma warning disable 618
            var dbType = SqlMapper.LookupDbType(fieldType, fieldName, false, out var typeHandler);
#pragma warning restore 618
            if (typeHandler is ITypeHandlerColumnType typeHandlerColumnType)
            {
                dbType = typeHandlerColumnType.ColumnType;
                fieldLength = typeHandlerColumnType.Length;
            }

            if (dbType == DbType.AnsiString ||
                dbType == DbType.AnsiStringFixedLength ||
                dbType == DbType.String ||
                dbType == DbType.StringFixedLength)
            {
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat,
                    fieldLength ?? DefaultStringLength);
            }
            else
            {
                if (!DbTypeMap.ColumnDbTypeMap.TryGetValue(dbType, out fieldDefinition))
                {
                    fieldDefinition = GetUndefinedColumnDefinition(fieldType, fieldLength);
                }
            }

            return fieldDefinition ?? GetUndefinedColumnDefinition(fieldType, null);
        }

        /// <summary>Query if 'dbCmd' does table exist.</summary>
        /// <param name="connection">       The database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        public virtual bool DoesTableExist(IDbConnection connection, string tableName)
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
        public virtual string GetColumnNames(ModelDefinition modelDef)
        {
            var sqlColumns = new StringBuilder();

            foreach (var fd in modelDef.FieldDefinitions)
            {
                if (sqlColumns.Length > 0)
                    sqlColumns.Append(", ");

                sqlColumns.Append(GetQuotedColumnName(fd.FieldName));

                if (fd.FieldName != fd.Name)
                {
                    sqlColumns.AppendFormat(" AS {0} ", fd.Name);
                }
            }

            return sqlColumns.ToString();
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

        /// <summary>Sequence list.</summary>
        /// <param name="modelDef">Model Definition.</param>
        /// <returns>A List&lt;string&gt;</returns>
        public virtual List<string> SequenceList(ModelDefinition modelDef)
        {
            return new List<string>();
        }

        /// <summary>Expression visitor.</summary>
        /// <exception cref="NotImplementedException">Thrown when the requested operation is unimplemented.</exception>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public virtual SqlExpressionVisitor<T> ExpressionVisitor<T>()
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets drop foreign key constraints.</summary>
        /// <param name="modelDef">The model definition.</param>
        /// <returns>The drop foreign key constraints.</returns>
        public virtual string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            return null;
        }

        /// <summary>Updates the string column definitions.</summary>
        private void UpdateStringColumnDefinitions()
        {
            StringLengthColumnDefinitionFormat = useUnicode
                ? StringLengthUnicodeColumnDefinitionFormat
                : StringLengthNonUnicodeColumnDefinitionFormat;

            StringColumnDefinition = string.Format(
                StringLengthColumnDefinitionFormat, DefaultStringLength);
        }

        /// <summary>Initialise the column type map.</summary>
        protected void InitColumnTypeMap()
        {
            DbTypeMap.Set(DbType.String, StringColumnDefinition);
            DbTypeMap.Set(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set(DbType.AnsiString, StringColumnDefinition);
            DbTypeMap.Set(DbType.AnsiStringFixedLength, StringColumnDefinition);
            DbTypeMap.Set(DbType.String, StringColumnDefinition);
            DbTypeMap.Set(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set(DbType.DateTimeOffset, DateTimeColumnDefinition);
            DbTypeMap.Set(DbType.DateTimeOffset, DateTimeColumnDefinition);

            DbTypeMap.Set(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set(DbType.UInt32, IntColumnDefinition);
            DbTypeMap.Set(DbType.UInt32, IntColumnDefinition);

            DbTypeMap.Set(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set(DbType.UInt64, LongColumnDefinition);
            DbTypeMap.Set(DbType.UInt64, LongColumnDefinition);

            DbTypeMap.Set(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set(DbType.Double, RealColumnDefinition);
            DbTypeMap.Set(DbType.Double, RealColumnDefinition);

            DbTypeMap.Set(DbType.Decimal, DecimalColumnDefinition);
            DbTypeMap.Set(DbType.Decimal, DecimalColumnDefinition);

            DbTypeMap.Set(DbType.Binary, BlobColumnDefinition);

            DbTypeMap.Set(DbType.Object, StringColumnDefinition);
            DbTypeMap.Set(DbType.Int32, IntColumnDefinition);
        }

        /// <summary>Gets undefined column definition.</summary>
        /// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
        /// <param name="fieldType">  Type of the field.</param>
        /// <param name="fieldLength">Length of the field.</param>
        /// <returns>The undefined column definition.</returns>
        protected virtual string GetUndefinedColumnDefinition(Type fieldType, int? fieldLength)
        {
            return string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
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
            return $"CREATE {(isUnique ? "UNIQUE" : string.Empty)} INDEX {indexName} ON {GetQuotedTableName(modelDef)} ({((isCombined) ? fieldName : GetQuotedColumnName(fieldName))} ASC); \n";
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
            return $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} MODIFY COLUMN {column};";
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
            return !skip.HasValue ? string.Empty : $"LIMIT {skip.Value}{(rows.HasValue ? $",{rows.Value}" : string.Empty)}";
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

        public virtual string GetDropTableStatement(ModelDefinition modelDef)
        {
            return "DROP TABLE " + GetQuotedTableName(modelDef);
        }
        public virtual IEnumerable<ColumnDefinition> TableColumnsInformation(IDbConnection connection, string tableName, string schemaName = null)
        {
            return new ColumnDefinition[0].AsEnumerable();
        }

        public virtual IEnumerable<TableDefinition> GetTablesInformation(IDbConnection connection, string dbName, string schemaName)
        {
            return new TableDefinition[0].AsEnumerable();
        }
    }
}