using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dapper;
using SimpleStack.Orm;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
	public static partial class SqlMapperLinq
	{
		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate, bool buffered = true)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Query<T>(ev.Where(predicate).ToSelectStatement(), ev.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression, bool buffered = true)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Query<T>(expression(ev).ToSelectStatement(), ev.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression, bool buffered = true)
		{
			return dbConn.Query<T>(expression.ToSelectStatement(), expression.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this OrmConnection dbConn, bool buffered = true)
		{
			return dbConn.Query<T>(dbConn.DialectProvider.ExpressionVisitor<T>().ToSelectStatement(), null, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T, V>(this OrmConnection dbConn, JoinSqlBuilder<T, V> sqlBuilder, bool buffered = true)
		{
			return dbConn.Query<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static T First<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Select(ev.Where(predicate).Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static T First<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return dbConn.Select(expression.Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static T FirstOrDefault<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Select(ev.Where(predicate).Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static T FirstOrDefault<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return dbConn.Select(expression.Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public static TKey GetScalar<T, TKey>(this OrmConnection dbConn, Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<TKey>(ev.Select(field).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public static TKey GetScalar<T, TKey>(this OrmConnection dbConn, Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<TKey>(ev.Where(predicate).Select(field).ToSelectStatement(), ev.Parameters);
		}

		public static long Count<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<long>(expression(ev).ToCountStatement(), ev.Parameters);
		}
		/// <summary>
		/// An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public static long Count<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return dbConn.ExecuteScalar<long>(expression.ToCountStatement(), expression.Parameters);
		}

		/// <summary>
		/// An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public static long Count<T>(this OrmConnection dbConn, Expression<Func<T, bool>> expression)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Count(ev.Where(expression));
		}

		/// <summary>
		/// An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <returns>A long.</returns>
		public static long Count<T>(this OrmConnection dbConn)
		{
			return dbConn.Count(dbConn.DialectProvider.ExpressionVisitor<T>());
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <returns>An int.</returns>
		public static int Update<T>(this OrmConnection dbConn,
			T model,
			Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
		{
			return dbConn.Update(model, onlyFields(dbConn.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="expression">The only fields.</param>
		/// <returns>An int.</returns>
		public static int Update<T>(this OrmConnection dbConn, 
			T model, 
			SqlExpressionVisitor<T> expression)
		{
			var cmd = dbConn.DialectProvider.ToUpdateRowStatement(model, expression);
			return dbConn.ExecuteScalar<int>(cmd);
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public static int Update<T, TKey>(this OrmConnection dbConn,
			T obj,
			Expression<Func<T, TKey>> onlyFields = null,
			Expression<Func<T, bool>> where = null)
		{
			if (onlyFields == null)
				throw new ArgumentNullException("onlyFields");

			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			ev.Update(onlyFields);
			ev.Where(where);
			return dbConn.Update(obj, ev);
		}

		/// <summary>An IDbConnection extension method that updates this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="updateOnly">The update only.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public static int Update<T>(this OrmConnection dbConn, 
			object updateOnly, 
			Expression<Func<T, bool>> where = null)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);

			var cmd = dbConn.DialectProvider.ToUpdateRowStatement(updateOnly, ev);
			return dbConn.ExecuteScalar<int>(cmd);
		}

		public static void Insert<T>(this OrmConnection dbConn, T obj)
		{
			dbConn.ExecuteScalar(dbConn.DialectProvider.ToInsertRowStatement(obj));
		}

		/// <summary>An IDbConnection extension method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="objs">  The objects.</param>
		public static void Insert<T>(this OrmConnection dbConn, IEnumerable<T> objs)
		{
			foreach (T t in objs)
			{
				dbConn.ExecuteScalar(dbConn.DialectProvider.ToInsertRowStatement<T>(t));
			}
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static void InsertOnly<T>(this OrmConnection dbConn, T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields) where T : new()
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			dbConn.InsertOnly(obj, onlyFields(ev));
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static void InsertOnly<T>(this OrmConnection dbConn, T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			var sql = dbConn.DialectProvider.ToInsertRowStatement(new[] { obj }, ev.InsertFields);
			dbConn.Execute(sql);
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this OrmConnection dbConn, Expression<Func<T, bool>> where)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Delete<T>(ev.Where(where));
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return dbConn.Delete<T>(where(dbConn.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> where)
		{
			return dbConn.ExecuteScalar<int>(where.ToDeleteRowStatement(), where.Parameters);
		}

		//TODO: Vdaron delete an item
		public static int Delete<T>(this OrmConnection dbConn, T obj)
		{
			//return dbConn.ExecuteScalar<int>(where.ToDeleteRowStatement(), where.Parameters);
			return 0;
		}

		public static int Delete<T>(this OrmConnection dbConn)
		{
			return dbConn.ExecuteScalar<int>(dbConn.DialectProvider.ExpressionVisitor<T>().ToDeleteRowStatement());
		}

		/// <summary>Alias for CreateTableIfNotExists.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		public static void CreateTable<T>(this OrmConnection dbConn, bool dropIfExists)
			where T : new()
		{
			var tableType = typeof(T);
			CreateTable(dbConn, dropIfExists, tableType);
		}

		/// <summary>Alias for CreateTableIfNotExists.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		public static void CreateTableIfNotExists<T>(this OrmConnection dbConn)
			where T : new()
		{
			var tableType = typeof(T);
			if (!TableExists<T>(dbConn))
			{
				CreateTable<T>(dbConn, false);
			}
		}

		public static void DropTable<T>(this OrmConnection dbConn)
		{
			var tableModelDef = typeof(T).GetModelDefinition();
			DropTable(dbConn, tableModelDef);
		}

		public static bool TableExists<T>(this OrmConnection dbConn)
		{
			var tableModelDef = typeof(T).GetModelDefinition();;
			return dbConn.DialectProvider.DoesTableExist(dbConn, dbConn.DialectProvider.NamingStrategy.GetTableName(tableModelDef.ModelName));
		}

		public static bool TableExists(this OrmConnection dbConn, string tableName)
		{
			return dbConn.DialectProvider.DoesTableExist(dbConn, tableName);
		}

		/// <summary>An IDbCommand extension method that creates a table.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="dbConn">    The dbCmd to act on.</param>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		/// <param name="modelType">Type of the model.</param>
		private static void CreateTable(this OrmConnection dbConn, bool overwrite, Type modelType)
		{
			var modelDef = modelType.GetModelDefinition();

			var dialectProvider = dbConn.DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
			var tableExists = dialectProvider.DoesTableExist(dbConn, tableName);

			if (overwrite && tableExists)
			{
				DropTable(dbConn, modelDef);
				tableExists = false;
			}

			try
			{
				if (!tableExists)
				{
					dbConn.Execute(dialectProvider.ToCreateTableStatement(modelDef));

					var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelDef);
					foreach (var sqlIndex in sqlIndexes)
					{
						dbConn.Execute(sqlIndex);
					}

					var sequenceList = dialectProvider.SequenceList(modelDef);
					if (sequenceList.Count > 0)
					{
						foreach (var seq in sequenceList)
						{
							if (dialectProvider.DoesSequenceExist(dbConn, seq) == false)
							{
								var seqSql = dialectProvider.ToCreateSequenceStatement(modelDef, seq);
								dbConn.Execute(seqSql);
							}
						}
					}
					else
					{
						var sequences = dialectProvider.ToCreateSequenceStatements(modelDef);
						foreach (var seq in sequences)
						{
							dbConn.Execute(seq);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		/// <summary>Drop table.</summary>
		/// <param name="dbConn">   The dbCmd to act on.</param>
		/// <param name="modelDef">The model definition.</param>
		private static void DropTable(OrmConnection dbConn, ModelDefinition modelDef)
		{
			try
			{
				var dialectProvider = dbConn.DialectProvider;
				var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);

				if (dbConn.DialectProvider.DoesTableExist(dbConn, tableName))
				{
					var dropTableFks = dbConn.DialectProvider.GetDropForeignKeyConstraints(modelDef);
					if (!string.IsNullOrEmpty(dropTableFks))
					{
						dbConn.Execute(dropTableFks);
					}
					dbConn.Execute(dbConn.DialectProvider.GetDropTableStatement(modelDef));
				}
			}
			catch (Exception ex)
			{
			}
		}
	}
}