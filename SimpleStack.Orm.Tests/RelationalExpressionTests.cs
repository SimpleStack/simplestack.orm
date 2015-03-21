using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	/// <summary>A relational expressions test.</summary>
	public partial class ExpressionTests
	{
		/// <summary>Can select greater than expression.</summary>
		[Test]
		public void Can_select_greater_than_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 1,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);

			var actual = OpenDbConnection().Select<TestType>(q => q.IntColumn > 1);

			Assert.IsNotNull(actual);
			Assert.AreEqual(10, actual.Count());
			CollectionAssert.DoesNotContain(actual, expected);
		}

		/// <summary>Can select greater or equal than expression.</summary>
		[Test]
		public void Can_select_greater_or_equal_than_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 1,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);

			var actual = OpenDbConnection().Select<TestType>(q => q.IntColumn >= 1);

			Assert.IsNotNull(actual);
			Assert.AreEqual(11, actual.Count());
			CollectionAssert.Contains(actual, expected);
		}

		/// <summary>Can select smaller than expression.</summary>
		[Test]
		public void Can_select_smaller_than_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 1,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);

			var actual = OpenDbConnection().Select<TestType>(q => q.IntColumn < 1);

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		/// <summary>Can select smaller or equal than expression.</summary>
		[Test]
		public void Can_select_smaller_or_equal_than_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 1,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);

			var actual = OpenDbConnection().Select<TestType>(q => q.IntColumn <= 1);

			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count());
			CollectionAssert.Contains(actual, expected);
		}
	}
}