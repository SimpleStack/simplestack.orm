using System;
using System.Transactions;
using SimpleStack.Orm.Attributes;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	public partial class ExpressionTests
	{
		public class ModelWithOnlyStringFields
		{
			/// <summary>Gets or sets the identifier.</summary>
			/// <value>The identifier.</value>
			public string Id { get; set; }

			/// <summary>Gets or sets the name.</summary>
			/// <value>The name.</value>
			public string Name { get; set; }

			/// <summary>Gets or sets the identifier of the album.</summary>
			/// <value>The identifier of the album.</value>
			public string AlbumId { get; set; }

			/// <summary>Gets or sets the name of the album.</summary>
			/// <value>The name of the album.</value>
			public string AlbumName { get; set; }

			/// <summary>Creates a new ModelWithOnlyStringFields.</summary>
			/// <param name="id">The identifier.</param>
			/// <returns>The ModelWithOnlyStringFields.</returns>
			public static ModelWithOnlyStringFields Create(string id)
			{
				return new ModelWithOnlyStringFields
				{
					Id = id,
					Name = "Name",
					AlbumId = "AlbumId",
					AlbumName = "AlbumName",
				};
			}
		}
		public class ModelWithIdAndName
		{
			/// <summary>
			/// Initializes a new instance of the NServiceKit.Common.Tests.Models.ModelWithIdAndName
			/// class.
			/// </summary>
			public ModelWithIdAndName()
			{
			}

			/// <summary>
			/// Initializes a new instance of the NServiceKit.Common.Tests.Models.ModelWithIdAndName
			/// class.
			/// </summary>
			/// <param name="id">The identifier.</param>
			public ModelWithIdAndName(int id)
			{
				Id = id;
				Name = "Name" + id;
			}

			/// <summary>Gets or sets the identifier.</summary>
			/// <value>The identifier.</value>
			[AutoIncrement]
			public int Id { get; set; }

			/// <summary>Gets or sets the name.</summary>
			/// <value>The name.</value>
			public string Name { get; set; }

			/// <summary>Creates a new ModelWithIdAndName.</summary>
			/// <param name="id">The identifier.</param>
			/// <returns>A ModelWithIdAndName.</returns>
			public static ModelWithIdAndName Create(int id)
			{
				return new ModelWithIdAndName(id);
			}

			/// <summary>Assert is equal.</summary>
			/// <param name="actual">  The actual.</param>
			/// <param name="expected">The expected.</param>
			public static void AssertIsEqual(ModelWithIdAndName actual, ModelWithIdAndName expected)
			{
				if (actual == null || expected == null)
				{
					Assert.That(actual == expected, Is.True);
					return;
				}

				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Name, Is.EqualTo(expected.Name));
			}

			/// <summary>
			/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />.
			/// </summary>
			/// <param name="other">The model with identifier and name to compare to this object.</param>
			/// <returns>
			/// true if the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />; otherwise, false.
			/// </returns>
			public bool Equals(ModelWithIdAndName other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return other.Id == Id && Equals(other.Name, Name);
			}

			/// <summary>
			/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>
			/// true if the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />; otherwise, false.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != typeof(ModelWithIdAndName)) return false;
				return Equals((ModelWithIdAndName)obj);
			}

			/// <summary>Serves as a hash function for a particular type.</summary>
			/// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
			public override int GetHashCode()
			{
				unchecked
				{
					return (Id * 397) ^ (Name != null ? Name.GetHashCode() : 0);
				}
			}
		}

		/// <summary>Transaction commit persists data to the database.</summary>
		[Test]
		public void Transaction_commit_persists_data_to_the_db()
		{

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.Insert(new ModelWithIdAndName(1));

				using (var dbTrans = db.BeginTransaction())
				{
					db.Insert(new ModelWithIdAndName(2));
					db.Insert(new ModelWithIdAndName(3));

					var rowsInTrans = db.Select<ModelWithIdAndName>();
					Assert.That(rowsInTrans, Has.Count.EqualTo(3));

					dbTrans.Commit();
				}

				var rows = db.Select<ModelWithIdAndName>();
				Assert.That(rows, Has.Count.EqualTo(3));
			}
		}

		/// <summary>Transaction rollsback if not committed.</summary>
		[Test]
		public void Transaction_rollsback_if_not_committed()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.Insert(new ModelWithIdAndName(1));

				using (var dbTrans = db.BeginTransaction())
				{
					db.Insert(new ModelWithIdAndName(2));
					db.Insert(new ModelWithIdAndName(3));

					var rowsInTrans = db.Select<ModelWithIdAndName>();
					Assert.That(rowsInTrans, Has.Count.EqualTo(3));
				}

				var rows = db.Select<ModelWithIdAndName>();
				Assert.That(rows, Has.Count.EqualTo(1));
			}
		}

		/// <summary>Transaction rollsback transactions to different tables.</summary>
		[Test]
		public void Transaction_rollsback_transactions_to_different_tables()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
				db.CreateTable<ModelWithOnlyStringFields>(true);

				db.Insert(new ModelWithIdAndName(1));

				using (var dbTrans = db.BeginTransaction())
				{
					db.Insert(new ModelWithIdAndName(2));
					db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
					db.Insert(ModelWithOnlyStringFields.Create("id3"));

					Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
					Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
					Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
				}

				Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(1));
				Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(0));
				Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(0));
			}
		}

		/// <summary>Transaction commits inserts to different tables.</summary>
		[Test]
		public void Transaction_commits_inserts_to_different_tables()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
				db.CreateTable<ModelWithOnlyStringFields>(true);

				db.Insert(new ModelWithIdAndName(1));

				using (var dbTrans = db.BeginTransaction())
				{
					db.Insert(new ModelWithIdAndName(2));
					db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
					db.Insert(ModelWithOnlyStringFields.Create("id3"));

					Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
					Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
					Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));

					dbTrans.Commit();
				}

				Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
				Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
				Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void TransactionScope_commit_persists_data_to_the_db()
		{

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.Insert(new ModelWithIdAndName(1));

				using (var scope = new TransactionScope())
				using (var db2 = _dialectProvider.CreateConnection(ConnectionString))
				{
					db2.Open();

					db2.Insert(new ModelWithIdAndName(2));
					db2.Insert(new ModelWithIdAndName(3));

					var rowsInTrans = db2.Select<ModelWithIdAndName>();
					Assert.That(rowsInTrans, Has.Count.EqualTo(3));

					scope.Complete();
				}

				var rows = db.Select<ModelWithIdAndName>();
				Assert.That(rows, Has.Count.EqualTo(3));
			}
		}
		[Test]
		public void TransactionScope_rollsback_if_not_committed()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.Insert(new ModelWithIdAndName(1));

				using (var scope = new TransactionScope())
				using (var db2 = _dialectProvider.CreateConnection(ConnectionString))
				{
					db2.Open();

					db2.Insert(new ModelWithIdAndName(2));
					db2.Insert(new ModelWithIdAndName(3));

					var rowsInTrans = db2.Select<ModelWithIdAndName>();
					Assert.That(rowsInTrans, Has.Count.EqualTo(3));
				}

				var rows = db.Select<ModelWithIdAndName>();
				Assert.That(rows, Has.Count.EqualTo(1));
			}
		}
		[Test]
		public void TransactionScope_rollsback_transactions_to_different_tables()
		{
			using (var db1 = OpenDbConnection())
			{
				db1.CreateTable<ModelWithIdAndName>(true);
				db1.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
				db1.CreateTable<ModelWithOnlyStringFields>(true);

				db1.Insert(new ModelWithIdAndName(1));

				using (var scope = new TransactionScope())
				using(var db = _dialectProvider.CreateConnection(ConnectionString))
				{
					db.Open();

					db.Insert(new ModelWithIdAndName(2));
					db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
					db.Insert(ModelWithOnlyStringFields.Create("id3"));

					Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
					Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
					Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
				}

				Assert.That(db1.Select<ModelWithIdAndName>(), Has.Count.EqualTo(1));
				Assert.That(db1.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(0));
				Assert.That(db1.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(0));
			}
		}
		[Test]
		public void TransactionScope_commits_inserts_to_different_tables()
		{
			
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
				db.CreateTable<ModelWithOnlyStringFields>(true);

				db.Insert(new ModelWithIdAndName(1));

				using (var scope = new TransactionScope())
				{
					db.Insert(new ModelWithIdAndName(2));
					db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
					db.Insert(ModelWithOnlyStringFields.Create("id3"));

					Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
					Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
					Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));

					scope.Complete();
				}

				Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
				Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
				Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
			}
		}

		/// <summary>my table.</summary>
		//class MyTable
		//{
		//	/// <summary>Gets or sets the identifier.</summary>
		//	/// <value>The identifier.</value>
		//	[AutoIncrement]
		//	public int Id { get; set; }

		//	/// <summary>Gets or sets some text field.</summary>
		//	/// <value>some text field.</value>
		//	public String SomeTextField { get; set; }
		//}

		/// <summary>Does sqlite transactions.</summary>
		//[Test]
		//public void Does_Sqlite_transactions()
		//{
		//	var factory = new OrmLiteConnectionFactory(":memory:", true, SqliteDialect.Provider);

		//	// test 1 - no transactions
		//	try
		//	{
		//		using (var conn = factory.OpenDbConnection())
		//		{
		//			conn.CreateTable<MyTable>();

		//			conn.Insert(new MyTable { SomeTextField = "Example" });
		//			var record = conn.GetById<MyTable>(1);
		//		}

		//		"Test 1 Success".Print();
		//	}
		//	catch (Exception e)
		//	{
		//		Assert.Fail("Test 1 Failed: {0}".Fmt(e.Message));
		//	}

		//	// test 2 - all transactions
		//	try
		//	{
		//		using (var conn = factory.OpenDbConnection())
		//		{
		//			conn.CreateTable<MyTable>();

		//			using (var tran = conn.OpenTransaction())
		//			{
		//				conn.Insert(new MyTable { SomeTextField = "Example" });
		//				tran.Commit();
		//			}

		//			using (var tran = conn.OpenTransaction())
		//			{
		//				var record = conn.GetById<MyTable>(1);
		//			}
		//		}

		//		"Test 2 Success".Print();
		//	}
		//	catch (Exception e)
		//	{
		//		Assert.Fail("Test 2 Failed: {0}".Fmt(e.Message));
		//	}

		//	// test 3 - transaction for insert, not for select
		//	try
		//	{
		//		using (var conn = factory.OpenDbConnection())
		//		{
		//			conn.CreateTable<MyTable>();

		//			using (var tran = conn.OpenTransaction())
		//			{
		//				conn.Insert(new MyTable { SomeTextField = "Example" });
		//				tran.Commit();
		//			}

		//			var record = conn.GetById<MyTable>(1);
		//		}

		//		"Test 3 Success".Print();
		//	}
		//	catch (Exception e)
		//	{
		//		Assert.Fail("Test 3 Failed: {0}".Fmt(e.Message));
		//	}
		//}

	}
}