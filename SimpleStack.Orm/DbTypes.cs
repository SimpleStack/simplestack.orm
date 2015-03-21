using System;
using System.Collections.Generic;
using System.Data;

namespace SimpleStack.Orm
{
    /// <summary>A database types.</summary>
    /// <typeparam name="TDialect">Type of the dialect.</typeparam>
	public class DbTypes<TDialect>
		where TDialect : IDialectProvider
	{
        /// <summary>Type of the database.</summary>
        public DbType DbType;

        /// <summary>The text definition.</summary>
        public string TextDefinition;

        /// <summary>true if should quote value.</summary>
        public bool ShouldQuoteValue;

        /// <summary>The column type map.</summary>
        public Dictionary<Type, string> ColumnTypeMap = new Dictionary<Type, string>();

        /// <summary>The column database type map.</summary>
        public Dictionary<Type, DbType> ColumnDbTypeMap = new Dictionary<Type, DbType>();

        /// <summary>Sets.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="dbType">         Type of the database.</param>
        /// <param name="fieldDefinition">The field definition.</param>
        public void Set<T>(DbType dbType, string fieldDefinition)
        {
            DbType = dbType;
            TextDefinition = fieldDefinition;
            ShouldQuoteValue = fieldDefinition != "INTEGER"
                && fieldDefinition != "BIGINT"
                && fieldDefinition != "DOUBLE"
                && fieldDefinition != "DECIMAL"
                && fieldDefinition != "BOOL";

			ColumnTypeMap[typeof(T)] = fieldDefinition;
            ColumnDbTypeMap[typeof(T)] = dbType;
        }
    }
}