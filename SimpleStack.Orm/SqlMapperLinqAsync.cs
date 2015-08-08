using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm
{
	public partial class OrmConnection
	{
		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.QueryAsync<T>(ev.Where(predicate).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.QueryAsync<T>(expression(ev).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(SqlExpressionVisitor<T> expression)
		{
			return await this.QueryAsync<T>(expression.ToSelectStatement(), expression.Parameters);
		}

		/// <summary>An OrmConnection extension method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>()
		{
			return await this.QueryAsync<T>(DialectProvider.ExpressionVisitor<T>().ToSelectStatement());
		}

		/// <summary>An OrmConnection extension method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="V"></typeparam>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T, V>(JoinSqlBuilder<T, V> sqlBuilder)
		{
			return await this.QueryAsync<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters);
		}

		/// <summary>An OrmConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var r = await SelectAsync(ev.Where(predicate).Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection extension method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstAsync<T>(SqlExpressionVisitor<T> expression)
		{
			var r = await SelectAsync(expression.Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var r = await SelectAsync(ev.Where(predicate).Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection extension method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstOrDefaultAsync<T>(SqlExpressionVisitor<T> expression)
		{
			var r = await SelectAsync(expression.Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<TKey>(ev.Select(field).ToSelectStatement(), ev.Parameters);
		}

		/// <summary>An OrmConnection extension method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<TKey>(ev.Where(predicate).Select(field).ToSelectStatement(), ev.Parameters);
		}

		public async Task<long> CountAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<long>(expression(ev).ToCountStatement(), ev.Parameters);
		}

		/// <summary>
		///    An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public async Task<long> CountAsync<T>(SqlExpressionVisitor<T> expression)
		{
			return await this.ExecuteScalarAsync<long>(expression.ToCountStatement(), expression.Parameters);
		}

		/// <summary>
		///    An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public async Task<long> CountAsync<T>(Expression<Func<T, bool>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await CountAsync(ev.Where(expression));
		}

		/// <summary>
		///    An OrmConnection extension method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A long.</returns>
		public async Task<long> CountAsync<T>()
		{
			return await CountAsync(DialectProvider.ExpressionVisitor<T>());
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="model">     The model.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <returns>An int.</returns>
		public async Task<int> UpdateAsync<T>(
			T model,
			Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
		{
			return await UpdateAsync(model, onlyFields(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="model">     The model.</param>
		/// <param name="expression">The only fields.</param>
		/// <returns>An int.</returns>
		public async Task<int> UpdateAsync<T>(
			T model,
			SqlExpressionVisitor<T> expression)
		{
			var cmd = DialectProvider.ToUpdateRowStatement(model, expression);
			return await this.ExecuteScalarAsync<int>(cmd);
		}

		/// <summary>An OrmConnection extension method that updates the only.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> UpdateAsync<T, TKey>(
			T obj,
			Expression<Func<T, TKey>> onlyFields,
			Expression<Func<T, bool>> where = null)
		{
			if (onlyFields == null)
				throw new ArgumentNullException(nameof(onlyFields));

			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Update(onlyFields);
			ev.Where(where);
			return await UpdateAsync(obj, ev);
		}

		/// <summary>An OrmConnection extension method that updates this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="updateOnly">The update only.</param>
		/// <param name="where">     The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> UpdateAsync<T>(
			object updateOnly,
			Expression<Func<T, bool>> where = null)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);

			var cmd = DialectProvider.ToUpdateRowStatement(updateOnly, ev);
			return await this.ExecuteScalarAsync<int>(cmd);
		}

		/// <summary>An OrmConnection extension method that inserts all.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="objs">  The objects.</param>
		public async Task<int> InsertAsync<T>(IEnumerable<T> objs)
		{
			var count = 0;
			foreach (var t in objs)
			{
				await this.ExecuteScalarAsync<int>(DialectProvider.ToInsertRowStatement(t));
				count++;
			}

			return count;
		}

		/// <summary>An OrmConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public async Task<int> InsertOnlyAsync<T>(T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
			where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await InsertOnlyAsync(obj, onlyFields(ev));
		}

		/// <summary>An OrmConnection extension method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public async Task<int> InsertOnlyAsync<T>(T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var sql = DialectProvider.ToInsertRowStatement(new[] {obj}, ev.InsertFields);
			return await this.ExecuteAsync(sql);
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> where)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await DeleteAsync(ev.Where(where));
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return await DeleteAsync(where(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An OrmConnection extension method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAsync<T>(SqlExpressionVisitor<T> where)
		{
			return await this.ExecuteScalarAsync<int>(where.ToDeleteRowStatement(), where.Parameters);
		}

		public async Task<int> DeleteAllAsync<T>()
		{
			return await this.ExecuteScalarAsync<int>(DialectProvider.ExpressionVisitor<T>().ToDeleteRowStatement());
		}
	}
}