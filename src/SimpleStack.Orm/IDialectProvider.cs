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
using SimpleStack.Orm.Expressions.Statements;

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
		/// <summary>Gets or sets the naming strategy.</summary>
		/// <value>The naming strategy.</value>
		INamingStrategy NamingStrategy { get; set; }
		
		IDbTypeMapper TypesMapper { get; set; }

		string GetParameterName(int parameterCount);

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
		

		CommandDefinition ToInsertStatement(InsertStatement insertStatement, CommandFlags flags);

		CommandDefinition ToSelectStatement(SelectStatement statement, CommandFlags flags);

        CommandDefinition ToUpdateStatement(UpdateStatement statement, CommandFlags flags);
        
        CommandDefinition ToCountStatement(CountStatement statement, CommandFlags flags);
		
		CommandDefinition ToDeleteStatement(DeleteStatement deleteStatement);

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
		
		/// <summary>Query if 'dbCmd' does sequence exist.</summary>
		/// <param name="dbCmd">      The database command.</param>
		/// <param name="sequencName">Name of the sequenc.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		bool DoesSequenceExist(IDbConnection dbCmd, string sequencName);

		/// <summary>Gets column names.</summary>
		/// <param name="modelDef">The model definition.</param>
		/// <returns>The column names.</returns>
		IEnumerable<string> GetColumnNames(ModelDefinition modelDef);

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
        /// </summary>
        /// <param name="name"></param>
        /// <param name="quotedColName"></param>
        /// <returns></returns>
		string GetDatePartFunction(string name, string quotedColName);
        IEnumerable<IColumnDefinition> GetTableColumnDefinitions(IDbConnection connection, string tableName, string schemaName = null);
        IEnumerable<ITableDefinition> GetTableDefinitions(IDbConnection connection, string schemaName = null);

        string BindOperand(ExpressionType e, bool isIntegral);

        string GetStringFunction(string functionName, string columnName, IDictionary<string,object> parameters, params string[] availableParameters);
        
	}
}