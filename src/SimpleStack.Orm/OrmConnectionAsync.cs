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
		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <param name="flags"></param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate, CommandFlags flags = CommandFlags.Buffered)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(ev.Where(predicate),flags));
		}

		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression, CommandFlags flags = CommandFlags.Buffered)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(expression(ev),flags));
		}

		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(SqlExpressionVisitor<T> expression, CommandFlags flags = CommandFlags.Buffered)
		{
			return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(expression,flags));
		}

		/// <summary>An OrmConnection method that selects.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T>(CommandFlags flags = CommandFlags.Buffered)
		{
			return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(DialectProvider.ExpressionVisitor<T>(),flags));
		}

		/// <summary>An OrmConnection method that selects based on a JoinSqlBuilder.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="V"></typeparam>
		/// <returns>A List&lt;T&gt;</returns>
		public async Task<IEnumerable<T>> SelectAsync<T, V>(JoinSqlBuilder<T, V> sqlBuilder)
		{
			return await this.QueryAsync<T>(sqlBuilder.ToSql(), sqlBuilder.Parameters);
		}

		/// <summary>An OrmConnection method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var r = await SelectAsync(ev.Where(predicate).Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection method that firsts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstAsync<T>(SqlExpressionVisitor<T> expression)
		{
			var r = await SelectAsync(expression.Limit(1));
			return r.First();
		}

		/// <summary>An OrmConnection method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var r = await SelectAsync(ev.Where(predicate).Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection method that first or default.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A T.</returns>
		public async Task<T> FirstOrDefaultAsync<T>(SqlExpressionVisitor<T> expression)
		{
			var r = await SelectAsync(expression.Limit(1));
			return r.FirstOrDefault();
		}

		/// <summary>An OrmConnection method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field"> The field.</param>
		/// <returns>The scalar.</returns>
		public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<TKey>(DialectProvider.ToSelectStatement(ev.Select(field),CommandFlags.None));
		}

		/// <summary>An OrmConnection method that gets a scalar.</summary>
		/// <typeparam name="T">   Generic type parameter.</typeparam>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="field">    The field.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The scalar.</returns>
		public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			//int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<TKey>(DialectProvider.ToSelectStatement(ev.Where(predicate).Select(field),CommandFlags.None));
		}

		public async Task<long> CountAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await this.ExecuteScalarAsync<long>(DialectProvider.ToCountStatement(expression(ev)));
		}

		/// <summary>
		///    An OrmConnection method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>A long.</returns>
		public async Task<long> CountAsync<T>(SqlExpressionVisitor<T> expression)
		{
			return await this.ExecuteScalarAsync<long>(DialectProvider.ToCountStatement(expression));
		}

		/// <summary>
		///    An OrmConnection method that counts the given database connection.
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
		///    An OrmConnection method that counts the given database connection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A long.</returns>
		public async Task<long> CountAsync<T>()
		{
			return await CountAsync(DialectProvider.ExpressionVisitor<T>());
		}

		public async Task<int> UpdateAsync<T>(T model)
		{
			var cmd = DialectProvider.ToUpdateRowStatement(model, DialectProvider.ExpressionVisitor<T>());
			return await this.ExecuteScalarAsync<int>(cmd);
		}

		public async Task<int> UpdateAsync<T, TKey>(T model, Expression<Func<T, TKey>> onlyFields)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Update(onlyFields);
			return await UpdateAsync(model, ev);
		}

		internal async Task<int> UpdateAsync<T>(T model, SqlExpressionVisitor<T> ev)
		{
			var cmd = DialectProvider.ToUpdateRowStatement(model, ev);
			return await this.ExecuteScalarAsync<int>(cmd);
		}

		public async Task<int> UpdateAllAsync<T, TKey>(T obj, Expression<Func<T, TKey>> onlyField, Expression<Func<T, bool>> where = null)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);
			if (onlyField != null)
			{
				ev.Update(onlyField);
			}
			return await UpdateAllAsync<T>(obj, ev);
		}

		public async Task<int> UpdateAllAsync<T>(object obj, Expression<Func<T, bool>> where = null, Expression<Func<T, object>> onlyField = null)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			ev.Where(where);
			if (onlyField != null)
			{
				ev.Update(onlyField);
			}
			return await UpdateAllAsync<T>(obj, ev);
		}

		internal async Task<int> UpdateAllAsync<T>(object obj, SqlExpressionVisitor<T> ev)
		{
			var cmd = DialectProvider.ToUpdateAllRowStatement(obj, ev);
			return await this.ExecuteScalarAsync<int>(cmd);
		}

		public async Task<int> InsertAsync<T>(T obj)
		{
			return await this.ExecuteScalarAsync<int>(DialectProvider.ToInsertRowStatement(obj));
		}

		/// <summary>An OrmConnection method that inserts all.</summary>
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

		/// <summary>An OrmConnection method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public async Task<int> InsertOnlyAsync<T>(T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
			where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			return await InsertOnlyAsync(obj, onlyFields(ev));
		}

		/// <summary>An OrmConnection method that inserts an only.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="obj">       The object.</param>
		/// <param name="onlyFields">The only fields.</param>
		public async Task<int> InsertOnlyAsync<T>(T obj, SqlExpressionVisitor<T> onlyFields) where T : new()
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			var sql = DialectProvider.ToInsertRowStatement(new[] {obj}, ev.InsertFields);
			return await this.ExecuteAsync(sql);
		}

		/// <summary>An OrmConnection method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAllAsync<T>(Expression<Func<T, bool>> where = null)
		{
			var ev = DialectProvider.ExpressionVisitor<T>();
			if (where != null)
				ev.Where(where);
			return await DeleteAllAsync(ev.Where(where));
		}

		/// <summary>An OrmConnection method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAllAsync<T>(Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
		{
			return await DeleteAllAsync(where(DialectProvider.ExpressionVisitor<T>()));
		}

		/// <summary>An OrmConnection method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAllAsync<T>(SqlExpressionVisitor<T> where)
		{
			return await this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteRowStatement(where));
		}

		/// <summary>An OrmConnection method that deletes this object.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where"> The where.</param>
		/// <returns>An int.</returns>
		public async Task<int> DeleteAsync<T>(T obj)
		{
			return await this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteRowStatement(obj));
		}
	}
}