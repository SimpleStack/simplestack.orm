using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SimpleStack.Orm;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
	public static partial class SqlMapperLinq
	{
		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static async Task<IEnumerable<T>> SelectAsync<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.QueryAsync<T>(ev.Where(predicate).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static async Task<IEnumerable<T>> SelectAsync<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.QueryAsync<T>(expression(ev).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static async Task<IEnumerable<T>> SelectAsync<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return await dbConn.QueryAsync<T>(expression.ToSelectStatement(), expression.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static async Task<IEnumerable<T>> SelectAsync<T>(this OrmConnection dbConn)
		{
			return await dbConn.QueryAsync<T>(dbConn.DialectProvider.ExpressionVisitor<T>().ToSelectStatement());
		}

		/// <summary>An OrmConnection extension method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public static async Task<IEnumerable<T>> SelectAsync<T, V>(this OrmConnection dbConn, JoinSqlBuilder<T, V> sqlBuilder)
		{
			return await dbConn.QueryAsync<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters);
		}

		/// <summary>An OrmConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static async Task<T> FirstAsync<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			var r = await dbConn.SelectAsync(ev.Where(predicate).Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static async Task<T> FirstAsync<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			var r = await dbConn.SelectAsync(expression.Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public static async Task<T> FirstOrDefaultAsync<T>(this OrmConnection dbConn, Expression<Func<T, bool>> predicate)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			var r = await dbConn.SelectAsync(ev.Where(predicate).Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public static async Task<T> FirstOrDefaultAsync<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			var r = await dbConn.SelectAsync(expression.Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public static async Task<TKey> GetScalarAsync<T, TKey>(this OrmConnection dbConn, Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.ExecuteScalarAsync<TKey>(ev.Select(field).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">   The dbConn to act on.</param>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public static async Task<TKey> GetScalarAsync<T, TKey>(this OrmConnection dbConn, Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.ExecuteScalarAsync<TKey>(ev.Where(predicate).Select(field).ToSelectStatement(), ev.Parameters);
		}

		public static async Task<long> CountAsync<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.ExecuteScalarAsync<long>(expression(ev).ToCountStatement(), ev.Parameters);
		}
		/// <summary>
		/// An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public static async Task<long> CountAsync<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> expression)
		{
			return await dbConn.ExecuteScalarAsync<long>(expression.ToCountStatement(), expression.Parameters);
		}

		/// <summary>
		/// An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public static async Task<long> CountAsync<T>(this OrmConnection dbConn, Expression<Func<T, bool>> expression)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.CountAsync(ev.Where(expression));
		}

		/// <summary>
		/// An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <returns>A long.</returns>
		public static async Task<long> CountAsync<T>(this OrmConnection dbConn)
		{
			return await dbConn.CountAsync(dbConn.DialectProvider.ExpressionVisitor<T>());
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <returns>An int.</returns>
		public static async Task<int> UpdateAsync<T>(this OrmConnection dbConn,
			T model,
			Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
		{
			return await dbConn.UpdateAsync(model, onlyFields(dbConn.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="model">     The model.</param>
		/// <param name="expression">The only fields.</param>
		/// <returns>An int.</returns>
		public static async Task<int> UpdateAsync<T>(this OrmConnection dbConn, 
			T model, 
			SqlExpressionVisitor<T> expression)
		{
			var cmd = dbConn.DialectProvider.ToUpdateRowStatement(model, expression);
			return await dbConn.ExecuteScalarAsync<int>(cmd);
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public static async Task<int> UpdateAsync<T, TKey>(this OrmConnection dbConn,
			T obj,
			Expression<Func<T, TKey>> onlyFields = null,
			Expression<Func<T, bool>> where = null)
		{
			if (onlyFields == null)
				throw new ArgumentNullException("onlyFields");

			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			ev.Update(onlyFields);
			ev.Where(where);
			return await dbConn.UpdateAsync(obj, ev);
		}

		/// <summary>An OrmConnection extension method that updates this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="updateOnly">The update only.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public static async Task<int> UpdateAsync<T>(this OrmConnection dbConn, 
			object updateOnly, 
			Expression<Func<T, bool>> where = null)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);

			var cmd = dbConn.DialectProvider.ToUpdateRowStatement(updateOnly, ev);
			return await dbConn.ExecuteScalarAsync<int>(cmd);
		}

		/// <summary>An OrmConnection extension method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="objs">  The objects.</param>
		public static async Task<int> InsertAsync<T>(this OrmConnection dbConn, IEnumerable<T> objs)
		{
			int count = 0;
			foreach (T t in objs)
			{
				await dbConn.ExecuteScalarAsync<int>(dbConn.DialectProvider.ToInsertRowStatement<T>(t));
				count++;
			}

			return count;
		}

		/// <summary>An OrmConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static async Task<int> InsertOnlyAsync<T>(this OrmConnection dbConn, T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields) where T : new()
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.InsertOnlyAsync(obj, onlyFields(ev));
		}

		/// <summary>An OrmConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">    The dbConn to act on.</param>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public static async Task<int> InsertOnlyAsync<T>(this OrmConnection dbConn, T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			var sql = dbConn.DialectProvider.ToInsertRowStatement(new[] { obj }, ev.InsertFields);
			return await dbConn.ExecuteAsync(sql);
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static async Task<int> DeleteAsync<T>(this OrmConnection dbConn, Expression<Func<T, bool>> where)
		{
			var ev = dbConn.DialectProvider.ExpressionVisitor<T>();
			return await dbConn.DeleteAsync<T>(ev.Where(where));
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static async Task<int> DeleteAsync<T>(this OrmConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return await dbConn.DeleteAsync<T>(where(dbConn.DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbConn">The dbConn to act on.</param>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public static async Task<int> DeleteAsync<T>(this OrmConnection dbConn, SqlExpressionVisitor<T> where)
		{
			return await dbConn.ExecuteScalarAsync<int>(where.ToDeleteRowStatement(), where.Parameters);
		}

		public static async Task<int> DeleteAsync<T>(this OrmConnection dbConn)
		{
			return await dbConn.ExecuteScalarAsync<int>(dbConn.DialectProvider.ExpressionVisitor<T>().ToDeleteRowStatement());
		}
	}
}