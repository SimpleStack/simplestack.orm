using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using SimpleStack.Orm.Expressions;
using SimpleStack.Orm.Expressions.Statements.Dynamic;
using SimpleStack.Orm.Expressions.Statements.Typed;

namespace SimpleStack.Orm
{
	public partial class OrmConnection
	{
        public IEnumerable<dynamic> Select(string tableName, Action<DynamicSelectStatement> selectStatement, CommandFlags flags = CommandFlags.Buffered)
        {
	        return SelectAsync(tableName, selectStatement, flags).Result;
        }

        /// <summary>An OrmConnection method that selects.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <param name="buffered"></param>
        /// <param name="flags"></param>
        /// <returns>A List&lt;T&gt;</returns>
        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate, CommandFlags flags = CommandFlags.Buffered)
        {
	        return SelectAsync(predicate, flags).Result;
		}

		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The ev.</param>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(Action<TypedSelectStatement<T>> expression, CommandFlags flags = CommandFlags.Buffered)
		{
			return SelectAsync(expression, flags).Result;
		}

		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(CommandFlags flags = CommandFlags.Buffered)
		{
			return SelectAsync<T>(flags).Result;
		}

		/// <summary>An OrmConnection method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="sqlBuilder"></param>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
//		public IEnumerable<T> Select<T, V>(JoinSqlBuilder<T, V> sqlBuilder, bool buffered = true)
//		{
//			return this.Query<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters, buffered: buffered);
//		}

		/// <summary>An OrmConnection method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public T First<T>(Expression<Func<T, bool>> predicate)
		{
			return FirstAsync(predicate).Result;
		}
		
		/// <summary>An OrmConnection method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public T First<T>(Action<TypedSelectStatement<T>> expression)
		{
			return FirstAsync(expression).Result;
		}

		/// <summary>An OrmConnection method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
		{
			return FirstOrDefaultAsync(predicate).Result;
		}

		/// <summary>An OrmConnection method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="expression">The ev.</param>
		/// <returns>A T.</returns>
		public T FirstOrDefault<T>(Action<TypedSelectStatement<T>> expression)
		{
			return FirstOrDefaultAsync(expression).Result;
		}

		/// <summary>An OrmConnection method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public TKey GetScalar<T, TKey>(Expression<Func<T, TKey>> field)
		{
			return GetScalarAsync(field).Result;
		}

		/// <summary>An OrmConnection method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public TKey GetScalar<T, TKey>(Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			return GetScalarAsync(field, predicate).Result;
		}

		/// <summary>
		///    An OrmConnection method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The ev.</param>
		/// <returns>A long.</returns>
		public long Count<T>(Action<TypedSelectStatement<T>> expression)
		{
			return CountAsync<T>(expression).Result;
		}

		/// <summary>
		///    An OrmConnection method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The ev.</param>
		/// <returns>A long.</returns>
		public long Count<T>(Expression<Func<T, bool>> expression)
		{
			return CountAsync<T>(expression).Result;
		}

		public long Count(string tableName, Action<DynamicSelectStatement> expression)
		{
			return CountAsync(tableName, expression).Result;
		}

		/// <summary>
		///    An OrmConnection method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		public long Count<T>()
		{
			return CountAsync<T>().Result;
		}
		
		/// <summary>
		/// Update all properties of object instance
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model">Object to update</param>
		/// <returns></returns>
		public int Update<T>(T model)
		{
			return UpdateAsync(model).Result;
		}

		/// <summary>
		/// Update only some fields of an object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="model">Object to update</param>
		/// <param name="onlyFields">Specify the fields to update. ie : x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}</param>
		/// <returns></returns>
		public int Update<T,TKey>(T model, Expression<Func<T, TKey>> onlyFields)
		{
			return UpdateAsync(model, onlyFields).Result;
		}

		/// <summary>
		/// Update all object of a given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="obj">New values</param>
		/// <param name="onlyField">Fields to update</param>
		/// <param name="where">Where clause</param>
		/// <returns></returns>
		public int UpdateAll<T>(object obj, Expression<Func<T, bool>> where = null, Expression<Func<T, object>> onlyField = null)
		{
			return UpdateAllAsync(obj,onlyField, where).Result;
		}
		public int UpdateAll<T>(T obj, Expression<Func<T, bool>> where = null, Expression<Func<T, object>> onlyField = null)
		{
			return UpdateAllAsync(obj,onlyField, where).Result;
		}

		public void Insert<T>(T obj)
		{
			InsertAsync(obj).Wait();
		}

		/// <summary>An OrmConnection method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="objs">  The objects.</param>
		public void Insert<T>(IEnumerable<T> objs)
		{
			InsertAsync(objs).Wait();
		}

		/// <summary>An OrmConnection method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public void InsertOnly<T>(T obj, Expression<Func<T, object>> onlyField = null) where T : new()
		{
			InsertOnlyAsync(obj,onlyField).Wait();
		}

		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public int DeleteAll<T>(Expression<Func<T, bool>> where = null)
		{
			return DeleteAllAsync(where).Result;
		}

//		/// <summary>An OrmConnection method that deletes this object.</summary>
//		/// <typeparam name="T">Generic type parameter.</typeparam>
//		/// <param name="where"> The where.</param>
//		/// <returns>An int.</returns>
//		public int DeleteAll<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
//		{
//			return DeleteAllAsync(where).Result;
//		}
//
//		/// <param name="where"> The where.</param>
//		/// <returns>An int.</returns>
//		public int DeleteAll<T>(SqlExpressionVisitor<T> where)
//		{
//			return DeleteAllAsync(where).Result;
//		}

		/// <summary>
		/// Delete a single item based on primary key values
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int Delete<T>(T obj)
		{
			return DeleteAsync(obj).Result;
		}

		public void CreateTable<T>(bool dropIfExists)
		{
			if(!dropIfExists && TableExists<T>())
				throw new OrmException("Table already exists");

			var tableType = typeof (T);
			CreateTable(dropIfExists, tableType);
		}

		public void CreateTableIfNotExists<T>()
		{
			var tableType = typeof(T);
			CreateTable(false, tableType);
		}

		public bool TableExists<T>()
		{
			var tableModelDef = typeof(T).GetModelDefinition();
			return DialectProvider.DoesTableExist(this,DialectProvider.NamingStrategy.GetTableName(tableModelDef.ModelName));
		}

		public bool TableExists(string tableName)
		{
			return DialectProvider.DoesTableExist(this, tableName);
		}

		public bool DropTableIfExists<T>()
		{
			if (TableExists<T>())
			{
				var tableModelDef = typeof(T).GetModelDefinition();
				DropTable(tableModelDef);
			}
			return false;
		}

		/// <summary>An IDbCommand method that creates a table.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		/// <param name="modelType">Type of the model.</param>
		private void CreateTable(bool overwrite, Type modelType)
		{
			var modelDef = modelType.GetModelDefinition();

			var dialectProvider = DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
			var tableExists = dialectProvider.DoesTableExist(this, tableName);

			if (overwrite && tableExists)
			{
				DropTable(modelDef);
				tableExists = false;
			}

			if (!tableExists)
			{
				this.Execute(dialectProvider.ToCreateTableStatement(modelDef));

				var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelDef);
				foreach (var sqlIndex in sqlIndexes)
				{
					this.Execute(sqlIndex);
				}

				var sequenceList = dialectProvider.SequenceList(modelDef);
				if (sequenceList.Count > 0)
				{
					foreach (var seq in sequenceList)
					{
						if (dialectProvider.DoesSequenceExist(this, seq) == false)
						{
							var seqSql = dialectProvider.ToCreateSequenceStatement(modelDef, seq);
							this.Execute(seqSql);
						}
					}
				}
				else
				{
					var sequences = dialectProvider.ToCreateSequenceStatements(modelDef);
					foreach (var seq in sequences)
					{
						this.Execute(seq);
					}
				}
			}
		}

		/// <summary>Drop table (Table MUST exists).</summary>
		/// <param name="modelDef">The model definition.</param>
		private void DropTable(ModelDefinition modelDef)
		{
			var dialectProvider = DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);

			var dropTableFks = DialectProvider.GetDropForeignKeyConstraints(modelDef);
			if (!string.IsNullOrEmpty(dropTableFks))
			{
				this.Execute(dropTableFks);
			}
			this.Execute(DialectProvider.GetDropTableStatement(modelDef));
		}

        public IEnumerable<ITableDefinition> GetTablesInformation(string schemaName = null)
        {
            return DialectProvider.GetTableDefinitions(DbConnection, schemaName);
        }
        public IEnumerable<IColumnDefinition> GetTableColumnsInformation(string tableName, string schemaName = null)
        {
            return DialectProvider.GetTableColumnDefinitions(DbConnection, tableName, schemaName);
        }
    }
}