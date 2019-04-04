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
using System.Linq.Expressions;
using Dapper;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
	/// <summary>Values that represent OnFkOption.</summary>
	public enum OnFkOption
	{
		/// <summary>An enum constant representing the cascade option.</summary>
		Cascade,
		/// <summary>An enum constant representing the set null option.</summary>
		SetNull,
		/// <summary>An enum constant representing the no action option.</summary>
		NoAction,
		/// <summary>An enum constant representing the set default option.</summary>
		SetDefault,
		/// <summary>An enum constant representing the restrict option.</summary>
		Restrict
	}

	/// <summary>Interface for ORM lite dialect provider.</summary>
	public interface IDialectProvider
	{
		/// <summary>Gets or sets the default string length.</summary>
		/// <value>The default string length.</value>
		int DefaultStringLength { get; set; }

		/// <summary>Gets or sets the parameter string.</summary>
		/// <value>The parameter string.</value>
		string ParamPrefix { get; set; }

		/// <summary>Gets or sets a value indicating whether this object use unicode.</summary>
		/// <value>true if use unicode, false if not.</value>
		bool UseUnicode { get; set; }

		/// <summary>Gets or sets the naming strategy.</summary>
		/// <value>The naming strategy.</value>
		INamingStrategy NamingStrategy { get; set; }

		string GetParameterName(int parameterCount);

		///// <summary>Gets quoted value.</summary>
		///// <param name="value">    The value.</param>
		///// <param name="fieldType">Type of the field.</param>
		///// <returns>The quoted value.</returns>
		//string GetQuotedValue(object value, Type fieldType);

		/// <summary>Creates a connection.</summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="options"> Options for controlling the operation.</param>
		/// <returns>The new connection.</returns>
		OrmConnection CreateConnection(string connectionString);

		/// <summary>Gets quoted table name.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The quoted table name.</returns>
		string GetQuotedTableName(ModelDefinition modelDef);

		/// <summary>Gets quoted table name.</summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>The quoted table name.</returns>
		string GetQuotedTableName(string tableName);

		/// <summary>Gets quoted column name.</summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The quoted column name.</returns>
		string GetQuotedColumnName(string columnName);

		/// <summary>Gets quoted name.</summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The quoted name.</returns>
		string GetQuotedName(string columnName);

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
		string GetColumnDefinition(
			string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement,
			bool isNullable, int? fieldLength,
			int? scale, string defaultValue);

		/// <summary>Converts this object to an insert row statement.</summary>
		/// <param name="command">          The command.</param>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <param name="InsertFields">     The insert fields.</param>
		/// <returns>The given data converted to a string.</returns>
		CommandDefinition ToInsertRowStatement<T>(T objWithProperties, ICollection<string> insertFields = null);// where T : new();

		/// <summary>Converts this object to an update row statement.</summary>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <param name="updateFields">     The update fields.</param>
		/// <returns>The given data converted to a string.</returns>
		CommandDefinition ToUpdateRowStatement<T>(T objWithProperties, SqlExpressionVisitor<T> visitor);

		/// <summary>Converts this object to an update row statement.</summary>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <param name="updateFields">     The update fields.</param>
		/// <returns>The given data converted to a string.</returns>
		CommandDefinition ToUpdateAllRowStatement<T>(object objWithProperties, SqlExpressionVisitor<T> visitor);

		/// <summary>Converts the objWithProperties to a delete row statement.</summary>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <returns>objWithProperties as a string.</returns>
		CommandDefinition ToDeleteRowStatement<T>(SqlExpressionVisitor<T> visitor);

		CommandDefinition ToDeleteRowStatement<T>(T objWithProperties);

		/// <summary>Converts this object to a delete statement.</summary>
		/// <param name="tableType">   Type of the table.</param>
		/// <param name="sqlFilter">   A filter specifying the SQL.</param>
		/// <param name="filterParams">Options for controlling the filter.</param>
		/// <returns>The given data converted to a string.</returns>
		//string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams);

        /// <summary>Converts this object to an exist statement.</summary>
		/// <param name="fromTableType">    Type of from table.</param>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <param name="sqlFilter">        A filter specifying the SQL.</param>
		/// <param name="filterParams">     Options for controlling the filter.</param>
		/// <returns>The given data converted to a string.</returns>
		//string ToExistStatement(Type fromTableType,
		//	object objWithProperties,
		//	string sqlFilter,
		//	params object[] filterParams);

		/// <summary>Converts this object to a select from procedure statement.</summary>
		/// <param name="fromObjWithProperties">from object with properties.</param>
		/// <param name="outputModelType">      Type of the output model.</param>
		/// <param name="sqlFilter">            A filter specifying the SQL.</param>
		/// <param name="filterParams">         Options for controlling the filter.</param>
		/// <returns>The given data converted to a string.</returns>
		//string ToSelectFromProcedureStatement(object fromObjWithProperties,
		//	Type outputModelType,
		//	string sqlFilter,
		//	params object[] filterParams);

		/// <summary>Converts this object to a count statement.</summary>
		/// <param name="fromTableType">Type of from table.</param>
		/// <param name="sqlFilter">    A filter specifying the SQL.</param>
		/// <param name="filterParams"> Options for controlling the filter.</param>
		/// <returns>The given data converted to a string.</returns>
		CommandDefinition ToCountStatement<T>(SqlExpressionVisitor<T> visitor);

		CommandDefinition ToSelectStatement<T>(SqlExpressionVisitor<T> visitor, CommandFlags flags);

		/// <summary>Converts the objWithProperties to an execute procedure statement.</summary>
		/// <param name="objWithProperties">The object with properties.</param>
		/// <returns>objWithProperties as a string.</returns>
		//string ToExecuteProcedureStatement(object objWithProperties);

        /// <summary>Converts a tableType to a create table statement.</summary>
		/// <param name="modelDefinition">Model Definition.</param>
		/// <returns>tableType as a string.</returns>
		string ToCreateTableStatement(ModelDefinition modelDefinition);

		/// <summary>Converts a tableType to a create index statements.</summary>
		/// <param name="modelDefinition">Model Definition.</param>
		/// <returns>tableType as a List&lt;string&gt;</returns>
		List<string> ToCreateIndexStatements(ModelDefinition modelDefinition);

		/// <summary>Converts a tableType to a create sequence statements.</summary>
		/// <param name="modelDefinition">Model Definition.</param>
		/// <returns>tableType as a List&lt;string&gt;</returns>
		List<string> ToCreateSequenceStatements(ModelDefinition modelDefinition);

		/// <summary>Converts this object to a create sequence statement.</summary>
		/// <param name="modelDefinition">Model Definition.</param>
		/// <param name="sequenceName">Name of the sequence.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToCreateSequenceStatement(ModelDefinition modelDefinition, string sequenceName);

		/// <summary>Sequence list.</summary>
		/// <param name="modelDefinition">Model Definition.</param>
		/// <returns>A List&lt;string&gt;</returns>
		List<string> SequenceList(ModelDefinition modelDefinition);

		/// <summary>Query if 'dbCmd' does table exist.</summary>
		/// <param name="connection">       The database.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		bool DoesTableExist(IDbConnection connection, string tableName);

		/// <summary>Query if 'dbCmd' does table exist.</summary>
		/// <param name="dbCmd">    The database command.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		//bool DoesTableExist(IDbCommand dbCmd, string tableName);

		/// <summary>Query if 'dbCmd' does sequence exist.</summary>
		/// <param name="dbCmd">      The database command.</param>
		/// <param name="sequencName">Name of the sequenc.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		bool DoesSequenceExist(IDbConnection dbCmd, string sequencName);

		/// <summary>Gets column names.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The column names.</returns>
		string GetColumnNames(ModelDefinition modelDef);

		/// <summary>Expression visitor.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		SqlExpressionVisitor<T> ExpressionVisitor<T>();

		/// <summary>Gets column type definition.</summary>
		/// <param name="fieldType">Type of the field.</param>
		/// <returns>The column type definition.</returns>
		string GetColumnTypeDefinition(Type fieldType, string fieldName, int? fieldLength);

		/// <summary>Gets drop foreign key constraints.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The drop foreign key constraints.</returns>
		string GetDropForeignKeyConstraints(ModelDefinition modelDef);


		string GetDropTableStatement(ModelDefinition modelDef);

		/// <summary>Converts this object to an add column statement.</summary>
		/// <param name="modelType">Type of the model.</param>
		/// <param name="fieldDef"> The field definition.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef);

		/// <summary>Converts this object to an alter column statement.</summary>
		/// <param name="modelType">Type of the model.</param>
		/// <param name="fieldDef"> The field definition.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef);

		/// <summary>Converts this object to a change column name statement.</summary>
		/// <param name="modelType">    Type of the model.</param>
		/// <param name="fieldDef">     The field definition.</param>
		/// <param name="oldColumnName">Name of the old column.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName);

		/// <summary>Converts this object to an add foreign key statement.</summary>
		/// <typeparam name="T">       Generic type parameter.</typeparam>
		/// <typeparam name="TForeign">Type of the foreign.</typeparam>
		/// <param name="field">         The field.</param>
		/// <param name="foreignField">  The foreign field.</param>
		/// <param name="onUpdate">      The on update.</param>
		/// <param name="onDelete">      The on delete.</param>
		/// <param name="foreignKeyName">Name of the foreign key.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
													 Expression<Func<TForeign, object>> foreignField,
													 OnFkOption onUpdate,
													 OnFkOption onDelete,
													 string foreignKeyName = null);

		/// <summary>Converts this object to a create index statement.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="field">    The field.</param>
		/// <param name="indexName">Name of the index.</param>
		/// <param name="unique">   true to unique.</param>
		/// <returns>The given data converted to a string.</returns>
		string ToCreateIndexStatement<T>(Expression<Func<T, object>> field,
										 string indexName = null, bool unique = false);

		string GetLimitExpression(int? skip, int? rows);

		/// <summary>Gets index name.</summary>
		/// <param name="isUnique"> true if this object is unique.</param>
		/// <param name="modelName">Name of the model.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>The index name.</returns>
		string GetIndexName(bool isUnique, string modelName, string fieldName);

        /// <summary>
        /// Return the Function to retrieve part of a date in SQL.
        /// The name is the date part requested
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="quotedColName"></param>
        /// <returns></returns>
		string GetDatePartFunction(string name, string quotedColName);
        IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection, string tableName, string schemaName = null);
        IEnumerable<TableDefinition> GetTableDefinitions(IDbConnection connection, string dbName, string schemaName = null);
    }
}