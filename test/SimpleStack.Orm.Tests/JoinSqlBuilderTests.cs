//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Dapper;
//using SimpleStack.Orm.Attributes;
//using NUnit.Framework;

//namespace SimpleStack.Orm.Tests
//{
//	/// <summary>A join SQL builder tests.</summary>
//	[TestFixture()]
//	public class JoinSqlBuilderTests
//	{
//		/// <summary>A with alias user.</summary>
//		[Alias("Users")]
//		public class WithAliasUser
//		{
//			/// <summary>Gets or sets the identifier.</summary>
//			/// <value>The identifier.</value>
//			[AutoIncrement]
//			public int Id { get; set; }

//			/// <summary>Gets or sets the name.</summary>
//			/// <value>The name.</value>
//			[Alias("Nickname")]
//			public string Name { get; set; }

//			/// <summary>Gets or sets the age.</summary>
//			/// <value>The age.</value>
//			[Alias("Agealias")]
//			public int Age { get; set; }
//		}

//		/// <summary>A with alias address.</summary>
//		[Alias("Addresses")]
//		public class WithAliasAddress
//		{
//			/// <summary>Gets or sets the identifier.</summary>
//			/// <value>The identifier.</value>
//			[AutoIncrement]
//			public int Id { get; set; }

//			/// <summary>Gets or sets the identifier of the user.</summary>
//			/// <value>The identifier of the user.</value>
//			public int UserId { get; set; }

//			/// <summary>Gets or sets the city.</summary>
//			/// <value>The city.</value>
//			public string City { get; set; }

//			/// <summary>Gets or sets the country.</summary>
//			/// <value>The country.</value>
//			[Alias("Countryalias")]
//			public string Country { get; set; }
//		}

//		/// <summary>An user.</summary>
//		public class User
//		{
//			/// <summary>Gets or sets the identifier.</summary>
//			/// <value>The identifier.</value>
//			[AutoIncrement]
//			public int Id { get; set; }

//			/// <summary>Gets or sets the name.</summary>
//			/// <value>The name.</value>
//			public string Name { get; set; }

//			/// <summary>Gets or sets the age.</summary>
//			/// <value>The age.</value>
//			public int Age { get; set; }
//		}

//		/// <summary>An address.</summary>
//		public class Address
//		{
//			/// <summary>Gets or sets the identifier.</summary>
//			/// <value>The identifier.</value>
//			[AutoIncrement]
//			public int Id { get; set; }

//			/// <summary>Gets or sets the identifier of the user.</summary>
//			/// <value>The identifier of the user.</value>
//			public int UserId { get; set; }

//			/// <summary>Gets or sets the city.</summary>
//			/// <value>The city.</value>
//			public string City { get; set; }

//			/// <summary>Gets or sets the country.</summary>
//			/// <value>The country.</value>
//			public string Country { get; set; }
//		}

//		/// <summary>Tests field name left join.</summary>
//		[Test()]
//		public void FieldNameLeftJoinTest()
//		{
//			var joinQuery = new JoinSqlBuilder<User, User>(new SqliteDialectProvider()).LeftJoin<User, Address>(x => x.Id, x => x.UserId).ToSql();
//			var expected =
//				"SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Address\" ON \"User\".\"Id\" = \"Address\".\"UserId\"  \n";

//			Assert.AreEqual(expected, joinQuery);

//			joinQuery =
//				new JoinSqlBuilder<WithAliasUser, WithAliasUser>(new SqliteDialectProvider()).LeftJoin<WithAliasUser, WithAliasAddress>(x => x.Id,
//					x => x.UserId).ToSql();
//			expected =
//				"SELECT \"Users\".\"Id\",\"Users\".\"Nickname\",\"Users\".\"Agealias\" \nFROM \"Users\" \n LEFT OUTER JOIN  \"Addresses\" ON \"Users\".\"Id\" = \"Addresses\".\"UserId\"  \n";

//			Assert.AreEqual(expected, joinQuery);


//			joinQuery = new JoinSqlBuilder<User, User>(new SqliteDialectProvider()).LeftJoin<User, WithAliasAddress>(x => x.Id, x => x.UserId).ToSql();
//			expected =
//				"SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \n";

//			Assert.AreEqual(expected, joinQuery);
//		}

//		/// <summary>Tests double where left join.</summary>
//		[Test()]
//		public void DoubleWhereLeftJoinTest()
//		{
//			var joinQuery = new JoinSqlBuilder<User, User>(new SqliteDialectProvider()).LeftJoin<User, WithAliasAddress>(x => x.Id, x => x.UserId
//				, sourceWhere: x => x.Age > 18
//				, destinationWhere: x => x.Country == "Italy");
//			var expected =
//				"SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \nWHERE (\"User\".\"Age\" > @p_0) AND (\"Addresses\".\"Countryalias\" = @p_1) \n";

//			Assert.AreEqual(expected, joinQuery.ToSql());
//			Assert.AreEqual(2, joinQuery.Parameters.Count);
//			Assert.AreEqual(18, joinQuery.Parameters["@p_0"]);
//			Assert.AreEqual("Italy", joinQuery.Parameters["@p_1"]);
//		}

//		[Test()]
//		public void SelectCountDistinctTest()
//		{
//			var joinQuery =
//				new JoinSqlBuilder<User, User>(new SqliteDialectProvider()).LeftJoin<User, Address>(x => x.Id, x => x.UserId)
//					.SelectCountDistinct<User>(x => x.Id).ToSql();
//			var expected =
//				"SELECT  COUNT(DISTINCT \"User\".\"Id\")  \nFROM \"User\" \n LEFT OUTER JOIN  \"Address\" ON \"User\".\"Id\" = \"Address\".\"UserId\"  \n";

//			Assert.AreEqual(expected, joinQuery);
//		}

//		[Test()]
//		public void SelectCountTest()
//		{
//			var joinQuery =
//				new JoinSqlBuilder<User, User>(new SqliteDialectProvider()).LeftJoin<User, Address>(x => x.Id, x => x.UserId)
//					.SelectCount<User>(x => x.Id).ToSql();
//			var expected =
//				"SELECT  COUNT(\"User\".\"Id\")  \nFROM \"User\" \n LEFT OUTER JOIN  \"Address\" ON \"User\".\"Id\" = \"Address\".\"UserId\"  \n";

//			Assert.AreEqual(expected, joinQuery);
//		}



//		public class Bar
//		{
//			[PrimaryKey]
//			public int Id { get; set; }

//			[Alias("Test")]
//			public string BarProperty { get; set; }
//		}

//		public class Foo
//		{
//			[PrimaryKey]
//			public int Id { get; set; }

//			public string FooProperty { get; set; }
//		}

//		public class BarView
//		{
//			public string BarProperty { get; set; }

//			public string FooProperty { get; set; }
//		}

//		[Test]
//		public void JoinSqlBuilderWithWhereCondition()
//		{
//			IDialectProvider dialectProvider = new SqliteDialectProvider();
//			using (OrmConnection connection = dialectProvider.CreateConnection(":memory:"))
//			{
//				connection.Open();

//				connection.CreateTable<Bar>(true);
//				for (int i = 0; i < 10; i++)
//				{
//					connection.Insert(new Bar { Id = i, BarProperty = i + "_Bar" });
//				}

//				connection.CreateTable<Foo>(true);
//				for (int i = 0; i < 10; i++)
//				{
//					connection.Insert(new Foo { Id = i, FooProperty = i + "_Foo" });
//				}

//				var builder = new JoinSqlBuilder<Bar, Bar>(dialectProvider)
//					 .Join<Bar, Foo>(x => x.Id, x => x.Id)
//					 .Select<Bar>(x => new { x.BarProperty })
//					 .Select<Foo>(x => new { x.FooProperty })
//					 .Where<Bar>(x => x.Id > 5);
//				IList<BarView> views = connection.Query<BarView>(builder.ToSql(),builder.Parameters).ToList();
//				Assert.AreEqual(4, views.Count);
//				foreach (var view in views)
//				{
//					Assert.AreEqual(view.BarProperty.Replace("_Bar", string.Empty), view.FooProperty.Replace("_Foo", string.Empty));
//				}

//				connection.Close();
//			}
//		}

//		[Test]
//		public void Test()
//		{
//			IDialectProvider dialectProvider = new SqliteDialectProvider();
//			using (OrmConnection connection = dialectProvider.CreateConnection(":memory:"))
//			{
//				connection.Open();

//				connection.CreateTable<Bar>(true);
//				for (int i = 0; i < 10; i++)
//				{
//					connection.Insert(new Bar { Id = i, BarProperty = i + "_Bar" });
//				}
//				connection.CreateTable<Foo>(true);
//				for (int i = 0; i < 10; i++)
//				{
//					connection.Insert(new Foo { Id = i, FooProperty = i + "_Foo" });
//				}

//				var builder = new JoinSqlBuilder<Bar, Bar>(dialectProvider)
//					 .Join<Bar, Foo>(x => x.Id, x => x.Id)
//					 .SelectAll<Bar>()
//					 .Select<Foo>(x => new { x.FooProperty });
//				IList<BarView> views = connection.Query<BarView>(builder.ToSql(), builder.Parameters).ToList();

//				Assert.AreEqual(10, views.Count);
//				foreach (var view in views)
//				{
//					Assert.AreEqual(view.BarProperty.Replace("_Bar", string.Empty), view.FooProperty.Replace("_Foo", string.Empty));
//				}
//			}
//		}
//	}
//}

