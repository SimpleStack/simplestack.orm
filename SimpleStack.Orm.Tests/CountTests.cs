using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using SimpleStack.Orm.Expressions;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	public partial class ExpressionTests
	{
		/// <summary>Can do count with interface.</summary>
		[Test]
		public void CanDoCountWithInterface()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<CountTestTable>(true);

				db.Insert(new CountTestTable { Id = 1, StringValue = "Your string value" });

				var count = db.GetScalar<CountTestTable, long>(e => Sql.Count(e.Id));

				Assert.That(count, Is.EqualTo(1));

				count = Count<CountTestTable>(db);

				Assert.That(count, Is.EqualTo(1));

				count = CountByColumn<CountTestTable>(db);

				Assert.That(count, Is.EqualTo(0));

			}
		}

		/// <summary>Can do count with interface and predicate.</summary>
		[Test]
		public void CanDoCountWithInterfaceAndPredicate()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<CountTestTable>(true);
				db.Insert(new CountTestTable { Id = 1, StringValue = "Your string value" });

				Expression<Func<CountTestTable, bool>> exp = q => q.Id == 2;
				var count = Count(db, exp);
				Assert.That(count, Is.EqualTo(0));


				exp = q => q.Id == 1;
				count = Count(db, exp);
				Assert.That(count, Is.EqualTo(1));

				exp = q => q.CountColumn == null;
				count = Count(db, exp);
				Assert.That(count, Is.EqualTo(1));

				exp = q => q.CountColumn == null;
				count = CountByColumn(db, exp);
				Assert.That(count, Is.EqualTo(0));
			}
		}

		/// <summary>Counts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="db">The database.</param>
		/// <returns>An int.</returns>
		long Count<T>(OrmConnection db) where T : IHasId<int>, new()
		{
			T request = new T();
			return db.GetScalar<T, long>(e => Sql.Count(request.Id));
		}

		/// <summary>Count by column.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="db">The database.</param>
		/// <returns>The total number of by column.</returns>
		long CountByColumn<T>(OrmConnection db) where T : IHasCountColumn, new()
		{
			T request = new T();
			return db.GetScalar<T, long?>(e => Sql.Count(request.CountColumn)).Value;
		}

		/// <summary>Counts.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="db">       The database.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>An int.</returns>
		int Count<T>(OrmConnection db, Expression<Func<T, bool>> predicate) where T : IHasId<int>, new()
		{
			return db.GetScalar<T, int>(e => Sql.Count(e.Id), predicate);
		}

		/// <summary>Count by column.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="db">       The database.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The total number of by column.</returns>
		int CountByColumn<T>(OrmConnection db, Expression<Func<T, bool>> predicate) where T : IHasCountColumn, new()
		{
			return db.GetScalar<T, int?>(e => Sql.Count(e.CountColumn), predicate).Value;
		}

	}

	public interface IHasId<T>
	{
		T Id { get; set; }
	}
	/// <summary>Interface for has count column.</summary>
	public interface IHasCountColumn
	{
		/// <summary>Gets or sets the total number of column.</summary>
		/// <value>The total number of column.</value>
		int? CountColumn { get; set; }
	}

	/// <summary>A count test table.</summary>
	public class CountTestTable : IHasId<int>, IHasCountColumn
	{
		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.Tests.Expression.CountTestTable
		/// class.
		/// </summary>
		public CountTestTable() { }
		#region IHasId implementation
		/// <summary>Gets or sets the identifier.</summary>
		/// <value>The identifier.</value>
		public int Id { get; set; }

		/// <summary>Gets or sets the string value.</summary>
		/// <value>The string value.</value>
		[StringLength(40)]
		public string StringValue { get; set; }
		#endregion

		#region IHasCountColumn implementation
		/// <summary>Gets or sets the total number of column.</summary>
		/// <value>The total number of column.</value>
		public int? CountColumn { get; set; }
		#endregion
	}
}