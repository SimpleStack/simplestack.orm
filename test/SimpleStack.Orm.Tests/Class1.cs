using System.Collections;
using System.Linq;
using Dapper;
using NUnit.Framework;
using SimpleStacl.Orm;
using SimpleStack.Orm;
using SimpleStack.Orm.Expressions;
using SimpleStack.Orm.PostgreSQL;

namespace SimpleStacl.Orm.Tests
{
	[TestFixture]
    public class CalculatorTests
    {
  //      [TestCase(1, 1, 2)]
  //      [TestCase(-1, -1, -2)]
  //      [TestCase(100, 5, 105)]
  //      public void CanAddNumbers(int x, int y, int expected)
  //      {
  //          Assert.That(1, Is.EqualTo(1));
  //      }

		//[Theory]
		//public void SquareRootDefinition(bool num)
		//{
		//	Assume.That(num, Is.False);
		//	Assert.That(num, Is.True);
		//}

		public enum TestEnum { Val0,Val1 }
		public class TestType2 { public TestEnum EnumCol { get; set; } }

		[Test]
		public void Can_Select_using_object_Array_Contains([ValueSource(typeof(MyDataClass), nameof(MyDataClass.Dialectproviders))]IDialectProvider _dialectProvider)
		{
			var vals = new object[] { TestEnum.Val0, TestEnum.Val1 };

			var visitor1 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor1.Where(q => vals.Contains(q.EnumCol) || vals.Contains(q.EnumCol));
			var sql1 = _dialectProvider.ToSelectStatement(visitor1, CommandFlags.None);

			var visitor2 = _dialectProvider.ExpressionVisitor<TestType2>();
			visitor2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
			var sql2 = _dialectProvider.ToSelectStatement(visitor2, CommandFlags.None);

			Assert.AreEqual(sql1.CommandText, sql2.CommandText);
		}

		public class MyDataClass
		{
			public static IEnumerable Dialectproviders
			{
				get
				{
					yield return new PostgreSQLDialectProvider();
				}
			}
		}
	}
}
