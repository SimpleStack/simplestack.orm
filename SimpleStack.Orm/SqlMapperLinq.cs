using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
	public partial class OrmConnection
	{
		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <param name="buffered"></param>
		/// <param name="flags"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate, CommandFlags flags = CommandFlags.Buffered)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return this.Query<T>(DialectProvider.ToSelectStatement(ev.Where(predicate), flags));
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression,
			CommandFlags flags = CommandFlags.Buffered)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return this.Query<T>(DialectProvider.ToSelectStatement(expression(ev),flags));
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(SqlExpressionVisitor<T> expression, CommandFlags flags = CommandFlags.Buffered)
		{
			return this.Query<T>(DialectProvider.ToSelectStatement(expression,flags));
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T>(CommandFlags flags = CommandFlags.Buffered)
		{
			return this.Query<T>(DialectProvider.ToSelectStatement(DialectProvider.ExpressionVisitor<T>(),flags));
		}

		/// <summary>An IDbConnection extension method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="sqlBuilder"></param>
		/// <param name="buffered"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public IEnumerable<T> Select<T, V>(JoinSqlBuilder<T, V> sqlBuilder, bool buffered = true)
		{
			return this.Query<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public T First<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return Select(ev.Where(predicate).Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public T First<T>(SqlExpressionVisitor<T> expression)
		{
			return Select(expression.Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return Select(ev.Where(predicate).Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public T FirstOrDefault<T>(SqlExpressionVisitor<T> expression)
		{
			return Select(expression.Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public TKey GetScalar<T, TKey>(Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = DialectProvider.ExpressionVisitor<T>();
			return this.ExecuteScalar<TKey>(DialectProvider.ToSelectStatement(ev.Select(field),CommandFlags.None));
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public TKey GetScalar<T, TKey>(Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = DialectProvider.ExpressionVisitor<T>();
			return this.ExecuteScalar<TKey>(DialectProvider.ToSelectStatement(ev.Where(predicate).Select(field),CommandFlags.None));
		}

		public long Count<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return this.ExecuteScalar<long>(DialectProvider.ToCountStatement(expression(ev)));
		}

		/// <summary>
		///    An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public long Count<T>(SqlExpressionVisitor<T> expression)
		{
			return this.ExecuteScalar<long>(DialectProvider.ToCountStatement(expression));
		}

		/// <summary>
		///    An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public long Count<T>(Expression<Func<T, bool>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return Count(ev.Where(expression));
		}

		/// <summary>
		///    An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		public long Count<T>()
		{
			return Count(DialectProvider.ExpressionVisitor<T>());
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="model">     The model.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <returns>An int.</returns>
		public int Update<T>(
			T model,
			Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
		{
			return Update(model, onlyFields(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="model">     The model.</param>
		/// <param name="expression">The only fields.</param>
		/// <returns>An int.</returns>
		public int Update<T>(
			T model,
			SqlExpressionVisitor<T> expression)
		{
			var cmd = DialectProvider.ToUpdateRowStatement(model, expression);
			return this.ExecuteScalar<int>(cmd);
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public int Update<T, TKey>(
			T obj,
			Expression<Func<T, TKey>> onlyFields,
			Expression<Func<T, bool>> where = null)
		{
			if (onlyFields == null)
				throw new ArgumentNullException(nameof(onlyFields));

			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Update(onlyFields);
			ev.Where(where);
			return Update(obj, ev);
		}

		/// <summary>An IDbConnection extension method that updates this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		
		/// <param name="updateOnly">The update only.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public int Update<T>(
			object updateOnly,
			Expression<Func<T, bool>> where = null)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);

			var cmd = DialectProvider.ToUpdateRowStatement(updateOnly, ev);
			return this.ExecuteScalar<int>(cmd);
		}

		public void Insert<T>(T obj)
		{
			this.ExecuteScalar(DialectProvider.ToInsertRowStatement(obj));
		}

		/// <summary>An IDbConnection extension method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="objs">  The objects.</param>
		public void Insert<T>(IEnumerable<T> objs)
		{
			foreach (var t in objs)
			{
				this.ExecuteScalar(DialectProvider.ToInsertRowStatement(t));
			}
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public void InsertOnly<T>(T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields) where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			InsertOnly(obj, onlyFields(ev));
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public void InsertOnly<T>(T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var sql = DialectProvider.ToInsertRowStatement(new[] {obj}, ev.InsertFields);
			this.Execute(sql);
		}

		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public int Delete<T>(Expression<Func<T, bool>> where)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return Delete(ev.Where(where));
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public int Delete<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return Delete(where(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public int Delete<T>(SqlExpressionVisitor<T> where)
		{
			return this.ExecuteScalar<int>(DialectProvider.ToDeleteRowStatement(where));
		}

		/// <summary>
		/// Delete a single item based on primary key values
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int Delete<T>(T obj)
		{
			return this.ExecuteScalar<int>(DialectProvider.ToDeleteRowStatement(obj));
		}

		/// <summary>
		/// Delete all object of a given type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int DeleteAll<T>()
		{
			return this.ExecuteScalar<int>(DialectProvider.ToDeleteRowStatement(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>Alias for CreateTableIfNotExists.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		public void CreateTable<T>(bool dropIfExists)
			where T : new()
		{
			var tableType = typeof (T);
			CreateTable(dropIfExists, tableType);
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

		/// <summary>An IDbCommand extension method that creates a table.</summary>
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
				DropTable(this, modelDef);
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

		/// <summary>Drop table.</summary>
		/// <param name="dbConn">   The dbCmd to act on.</param>
		/// <param name="modelDef">The model definition.</param>
		private void DropTable(OrmConnection dbConn, ModelDefinition modelDef)
		{
			var dialectProvider = DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);

			if (DialectProvider.DoesTableExist(dbConn, tableName))
			{
				var dropTableFks = DialectProvider.GetDropForeignKeyConstraints(modelDef);
				if (!string.IsNullOrEmpty(dropTableFks))
				{
					this.Execute(dropTableFks);
				}
				this.Execute(DialectProvider.GetDropTableStatement(modelDef));
			}
		}
	}
}