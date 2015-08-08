using System;
using System.Data;
using System.Linq;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	public partial class ExpressionTests
	{
		public class TestType2TypeHandler : SqlMapper.ITypeHandler
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				parameter.Value = ((TestType2)value).TextCol;
			}

			public object Parse(Type destinationType, object value)
			{
				return new TestType2() { TextCol = value.ToString() };
			}
		}

		public class JsonTypeHandler : SqlMapper.ITypeHandler
		{
			public void SetValue(IDbDataParameter parameter, object value)
			{
				parameter.DbType = DbType.String;
				parameter.Value = NServiceKit.Text.JsonSerializer.SerializeToString(value);
			}

			public object Parse(Type destinationType, object value)
			{
				return NServiceKit.Text.JsonSerializer.DeserializeFromString(value.ToString(), destinationType);
			}
		}

		private void SetupContext()
		{
			SqlMapper.AddTypeHandler(typeof(TestType2), new TestType2TypeHandler());
			SqlMapper.AddTypeHandler(typeof(TestEnum), new EnumAsStringTypeHandler<TestEnum>());

			OpenDbConnection().Insert(new TestType2 { Id = 1, BoolCol = true, DateCol = new DateTime(2012, 1, 1), TextCol = "asdf", EnumCol = TestEnum.Val0 });
			OpenDbConnection().Insert(new TestType2 { Id = 2, BoolCol = true, DateCol = new DateTime(2012, 2, 1), TextCol = "asdf123", EnumCol = TestEnum.Val1 });
			OpenDbConnection().Insert(new TestType2 { Id = 3, BoolCol = false, DateCol = new DateTime(2012, 3, 1), TextCol = "qwer", EnumCol = TestEnum.Val2 });
			OpenDbConnection().Insert(new TestType2 { Id = 4, BoolCol = false, DateCol = new DateTime(2012, 4, 1), TextCol = "qwer123", EnumCol = TestEnum.Val3 });
		}

		/// <summary>Can select by constant int.</summary>
		[Test]
		public void Can_Select_by_const_int()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.Id == 1);
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select by value returned by method without parameters.</summary>
		[Test]
		public void Can_Select_by_value_returned_by_method_without_params()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.Id == MethodReturningInt());
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select by value returned by method with parameter.</summary>
		[Test]
		public void Can_Select_by_value_returned_by_method_with_param()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.Id == MethodReturningInt(1));
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select by constant enum.</summary>
		[Test]
		public void Can_Select_by_const_enum()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.EnumCol == TestEnum.Val0);
			Assert.AreEqual(1, target.Count());
			target = OpenDbConnection().Select<TestType2>(q => TestEnum.Val0 == q.EnumCol);
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select by enum returned by method.</summary>
		[Test]
		public void Can_Select_by_enum_returned_by_method()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.EnumCol == MethodReturningEnum());
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select using to upper on string property of t.</summary>
		[Test]
		public void Can_Select_using_ToUpper_on_string_property_of_T()
		{
			SetupContext();
			var target =
				OpenDbConnection().Select<TestType2>(q => q.TextCol.ToUpper() == "ASDF");
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select using to lower on string property of field.</summary>
		[Test]
		public void Can_Select_using_ToLower_on_string_property_of_field()
		{
			SetupContext();
			var obj = new TestType2 { TextCol = "ASDF" };

			var target =
				OpenDbConnection().Select<TestType2>(q => q.TextCol == obj.TextCol.ToLower());
			Assert.AreEqual(1, target.Count());
		}

		/// <summary>Can select using constant bool value.</summary>
		[Test]
		public void Can_Select_using_Constant_Bool_Value()
		{
			SetupContext();
			var target =
				OpenDbConnection().Select<TestType2>(q => q.BoolCol == true);
			Assert.AreEqual(2, target.Count());
		}

		/// <summary>Can select using new.</summary>
		[Test]
		public void Can_Select_using_new()
		{
			SetupContext();

			using (var con = OpenDbConnection())
			{
				con.Insert(new TestType2
				{
					Id = 5,
					BoolCol = false,
					DateCol = new DateTime(2012, 5, 1),
					TextCol = "uiop",
					EnumCol = TestEnum.Val3,
					ComplexObjCol = new TestType2 { TextCol = "poiu" }
				});

				var target =
					OpenDbConnection().Select<TestType2>(
						q => q.ComplexObjCol == new TestType2() { TextCol = "poiu" });
				Assert.AreEqual(1, target.Count());
			}
		}

		/// <summary>Can select using in.</summary>
		[Test]
		public void Can_Select_using_IN()
		{
			SetupContext();

			var visitor = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor.Where(q => Sql.In(q.TextCol, "asdf", "qwer"));
			var target = OpenDbConnection().Select(visitor);
			Assert.AreEqual(2, target.Count());
		}

		/// <summary>Can select using in using parameters.</summary>
		[Test]
		public void Can_Select_using_IN_using_params()
		{
			SetupContext();

			var visitor = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor.Where(q => Sql.In(q.Id, 1, 2, 3));
			var target = OpenDbConnection().Select(visitor);
			Assert.AreEqual(3, target.Count());
		}

		/// <summary>Can select using in using int array.</summary>
		[Test]
		public void Can_Select_using_IN_using_int_array()
		{
			SetupContext();

			var visitor = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor.Where(q => Sql.In(q.Id, new[] { 1, 2, 3 }));
			var target = OpenDbConnection().Select(visitor);
			Assert.AreEqual(3, target.Count());
		}

		/// <summary>Can select using in using object array.</summary>
		[Test]
		public void Can_Select_using_IN_using_object_array()
		{
			SetupContext();

			var visitor = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor.Where(q => Sql.In(q.Id, new object[] { 1, 2, 3 }));
			var target = OpenDbConnection().Select(visitor);
			Assert.AreEqual(3, target.Count());
		}

		/// <summary>Can select using startswith.</summary>
		[Test]
		public void Can_Select_using_Startswith()
		{
			SetupContext();
			var target = OpenDbConnection().Select<TestType2>(q => q.TextCol.StartsWith("asdf"));
			Assert.AreEqual(2, target.Count());
		}

		/// <summary>Can selelct using chained string operations.</summary>
		[Test]
		public void Can_Selelct_using_chained_string_operations()
		{
			SetupContext();
			var value = "ASDF";
			var visitor = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor.Where(q => q.TextCol.ToUpper().StartsWith(value));
			var target = OpenDbConnection().Select(visitor);
			Assert.AreEqual(2, target.Count());
		}

		/// <summary>Can select using object array contains.</summary>
		[Test]
		public void Can_Select_using_object_Array_Contains()
		{
			SetupContext();
			var vals = new object[] { TestEnum.Val0, TestEnum.Val1 };

			var visitor1 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor1.Where(q => vals.Contains(q.EnumCol) || vals.Contains(q.EnumCol));
			var sql1 = _dialectProvider.ToSelectStatement(visitor1,CommandFlags.None);

			var visitor2 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
			var sql2 = _dialectProvider.ToSelectStatement(visitor2, CommandFlags.None);

			Assert.AreEqual(sql1.CommandText, sql2.CommandText);
		}

		/// <summary>Can select using int array contains.</summary>
		[Test]
		public void Can_Select_using_int_Array_Contains()
		{
			SetupContext();
			var vals = new int[] { (int)TestEnum.Val0, (int)TestEnum.Val1 };

			var visitor1 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor1.Where(q => vals.Contains((int)q.EnumCol) || vals.Contains((int)q.EnumCol));
			var sql1 = _dialectProvider.ToSelectStatement(visitor1,CommandFlags.None);

			var visitor2 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
			var sql2 = _dialectProvider.ToSelectStatement(visitor2, CommandFlags.None);

			Assert.AreEqual(sql1.CommandText, sql2.CommandText);
		}

		/// <summary>Method returning int.</summary>
		/// <param name="val">The value.</param>
		/// <returns>An int.</returns>
		private int MethodReturningInt(int val)
		{
			return val;
		}

		/// <summary>Method returning int.</summary>
		/// <returns>An int.</returns>
		private int MethodReturningInt()
		{
			return 1;
		}

		/// <summary>Method returning enum.</summary>
		/// <returns>A TestEnum.</returns>
		private TestEnum MethodReturningEnum()
		{
			return TestEnum.Val0;
		}
	}

	/// <summary>Values that represent TestEnum.</summary>
	public enum TestEnum
	{
		[Alias("ZERO")]
		/// <summary>An enum constant representing the value 0 option.</summary>
		Val0 = 0,

		[Alias("ONE")]
		/// <summary>An enum constant representing the value 1 option.</summary>
		Val1,

		[Alias("TWO")]
		/// <summary>An enum constant representing the value 2 option.</summary>
		Val2,

		[Alias("THREE")]
		/// <summary>An enum constant representing the value 3 option.</summary>
		Val3
	}

	/// <summary>A test type.</summary>
	public class TestType2
	{
		/// <summary>Gets or sets the identifier.</summary>
		/// <value>The identifier.</value>
		public int Id { get; set; }

		/// <summary>Gets or sets the text col.</summary>
		/// <value>The text col.</value>
		public string TextCol { get; set; }

		/// <summary>Gets or sets a value indicating whether the col.</summary>
		/// <value>true if col, false if not.</value>
		public bool BoolCol { get; set; }

		/// <summary>Gets or sets the Date/Time of the date col.</summary>
		/// <value>The date col.</value>
		public DateTime DateCol { get; set; }

		/// <summary>Gets or sets the enum col.</summary>
		/// <value>The enum col.</value>
		public TestEnum EnumCol { get; set; }

		/// <summary>Gets or sets the complex object col.</summary>
		/// <value>The complex object col.</value>
		public TestType2 ComplexObjCol { get; set; }
	}
}