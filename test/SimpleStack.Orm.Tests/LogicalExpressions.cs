using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	/// <summary>A logical expressions test.</summary>
	public partial class ExpressionTests
	{
		#region constants

		// Unlikely 
		// OpenDbConnection().Select<TestType>(q => q.BoolColumn == (true & false));

		#endregion

		#region variables
		/// <summary>Can select logical and variable expression.</summary>
		[Test]
		public void Can_select_logical_and_variable_expression()
		{
			// ReSharper disable ConvertToConstant.Local
			var a = true;
			var b = false;
			// ReSharper restore ConvertToConstant.Local

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (a & b));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select logical or variable expression.</summary>
		[Test]
		public void Can_select_logical_or_variable_expression()
		{
			// ReSharper disable ConvertToConstant.Local
			var a = true;
			var b = false;
			// ReSharper restore ConvertToConstant.Local

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (a | b));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select logical exclusive-or variable expression.</summary>
		[Test]
		public void Can_select_logical_xor_variable_expression()
		{
			// ReSharper disable ConvertToConstant.Local
			var a = true;
			var b = false;
			// ReSharper restore ConvertToConstant.Local

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (a ^ b));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		#endregion

		#region method
		/// <summary>Can select logical and method expression.</summary>
		[Test]
		public void Can_select_logical_and_method_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) & GetValue(false)));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select logical or method expression.</summary>
		[Test]
		public void Can_select_logical_or_method_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) | GetValue(false)));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select logical exclusive-or method expression.</summary>
		[Test]
		public void Can_select_logical_xor_method_expression()
		{
			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) ^ GetValue(false)));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 0);
				CollectionAssert.Contains(actual, expected);
			}
		}

		#endregion
	}
}