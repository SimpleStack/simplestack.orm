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
		public static IEnumerable<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, bool buffered = true)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Query<T>(ev.Where(predicate).ToSelectStatement(), ev.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression, bool buffered = true)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Query<T>(expression(ev).ToSelectStatement(), ev.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression, bool buffered = true)
		{
			return dbConn.Query<T>(expression.ToSelectStatement(), expression.Parameters, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static IEnumerable<T> Select<T>(this IDbConnection dbConn, bool buffered = true)
		{
			return dbConn.Query<T>(Config.DialectProvider.ExpressionVisitor<T>().ToSelectStatement(), null, buffered: buffered);
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static T First<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Select(ev.Where(predicate).Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static T First<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return dbConn.Select(expression.Limit(1)).First();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static T FirstOrDefault<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Select(ev.Where(predicate).Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static T FirstOrDefault<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return dbConn.Select(expression.Limit(1)).FirstOrDefault();
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<TKey>(ev.Select(field).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An IDbConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<TKey>(ev.Where(predicate).Select(field).ToSelectStatement(), ev.Parameters);
		}

		public static long Count<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.ExecuteScalar<long>(expression(ev).ToCountStatement(), ev.Parameters);
		}
		/// <summary>
		/// An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public static long Count<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
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
		public static long Count<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Count(ev.Where(expression));
		}

		/// <summary>
		/// An IDbConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <returns>A long.</returns>
		public static long Count<T>(this IDbConnection dbConn)
		{
			return dbConn.Count(Config.DialectProvider.ExpressionVisitor<T>());
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <returns>An int.</returns>
		public static int Update<T>(this IDbConnection dbConn,
			T model,
			Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
		{
			return dbConn.Update(model, onlyFields(Config.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An IDbConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="expression">The only fields.</param>
		/// <returns>An int.</returns>
		public static int Update<T>(this IDbConnection dbConn, 
			T model, 
			SqlExpressionVisitor<T> expression)
		{
			var cmd = Config.DialectProvider.ToUpdateRowStatement(model, expression);
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
		public static int Update<T, TKey>(this IDbConnection dbConn,
			T obj,
			Expression<Func<T, TKey>> onlyFields = null,
			Expression<Func<T, bool>> where = null)
		{
			if (onlyFields == null)
				throw new ArgumentNullException("onlyFields");

			var ev = Config.DialectProvider.ExpressionVisitor<T>();
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
		public static int Update<T>(this IDbConnection dbConn, 
			object updateOnly, 
			Expression<Func<T, bool>> where = null)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);

			var cmd = Config.DialectProvider.ToUpdateRowStatement(updateOnly, ev);
			return dbConn.ExecuteScalar<int>(cmd);
		}

		public static void Insert<T>(this IDbConnection dbConn, params T[] objs)
		{
			dbConn.ExecuteScalar(Config.DialectProvider.ToInsertRowStatement(objs));
		}

		/// <summary>An IDbConnection extension method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="objs">  The objects.</param>
		public static void Insert<T>(this IDbConnection dbConn, IEnumerable<T> objs)
		{
			dbConn.ExecuteScalar(Config.DialectProvider.ToInsertRowStatement(objs));
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static void InsertOnly<T>(this IDbConnection dbConn, T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields) where T : new()
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			dbConn.InsertOnly(obj, onlyFields(ev));
		}

		/// <summary>An IDbConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static void InsertOnly<T>(this IDbConnection dbConn, T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			var sql = Config.DialectProvider.ToInsertRowStatement(new[] { obj }, ev.InsertFields);
			dbConn.Execute(sql);
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this IDbConnection dbConn, Expression<Func<T, bool>> where)
		{
			var ev = Config.DialectProvider.ExpressionVisitor<T>();
			return dbConn.Delete<T>(ev.Where(where));
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return dbConn.Delete<T>(where(Config.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An IDbConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static int Delete<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> where)
		{
			return dbConn.ExecuteScalar<int>(where.ToDeleteRowStatement(), where.Parameters);
		}

		public static int Delete<T>(this IDbConnection dbConn)
		{
			return dbConn.ExecuteScalar<int>(Config.DialectProvider.ExpressionVisitor<T>().ToDeleteRowStatement());
		}

		/// <summary>Alias for CreateTableIfNotExists.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		public static void CreateTable<T>(this IDbConnection dbConn, bool dropIfExists)
			where T : new()
		{
			var tableType = typeof(T);
			CreateTable(dbConn, dropIfExists, tableType);
		}
		/// <summary>An IDbCommand extension method that creates a table.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="dbCmd">    The dbCmd to act on.</param>
		/// <param name="overwrite">true to overwrite, false to preserve.</param>
		/// <param name="modelType">Type of the model.</param>
		private static void CreateTable(this IDbConnection dbCmd, bool overwrite, Type modelType)
		{
			var modelDef = modelType.GetModelDefinition();

			var dialectProvider = Config.DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
			var tableExists = dialectProvider.DoesTableExist(dbCmd, tableName);

			if (overwrite && tableExists)
			{
				DropTable(dbCmd, modelDef);
				tableExists = false;
			}

			try
			{
				if (!tableExists)
				{
					dbCmd.Execute(dialectProvider.ToCreateTableStatement(modelType));

					var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelType);
					foreach (var sqlIndex in sqlIndexes)
					{
						dbCmd.Execute(sqlIndex);
					}

					var sequenceList = dialectProvider.SequenceList(modelType);
					if (sequenceList.Count > 0)
					{
						foreach (var seq in sequenceList)
						{
							if (dialectProvider.DoesSequenceExist(dbCmd, seq) == false)
							{
								var seqSql = dialectProvider.ToCreateSequenceStatement(modelType, seq);
								dbCmd.Execute(seqSql);
							}
						}
					}
					else
					{
						var sequences = dialectProvider.ToCreateSequenceStatements(modelType);
						foreach (var seq in sequences)
						{
							dbCmd.Execute(seq);
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
		/// <param name="dbCmd">   The dbCmd to act on.</param>
		/// <param name="modelDef">The model definition.</param>
		private static void DropTable(IDbConnection dbCmd, ModelDefinition modelDef)
		{
			try
			{

				var dialectProvider = Config.DialectProvider;
				var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);

				if (Config.DialectProvider.DoesTableExist(dbCmd, tableName))
				{
					var dropTableFks = Config.DialectProvider.GetDropForeignKeyConstraints(modelDef);
					if (!string.IsNullOrEmpty(dropTableFks))
					{
						dbCmd.Execute(dropTableFks);
					}
					dbCmd.Execute("DROP TABLE " + Config.DialectProvider.GetQuotedTableName(modelDef));
				}
			}
			catch (Exception ex)
			{
			}
		}
	}
}