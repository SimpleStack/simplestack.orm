using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	/// <summary>A primary expressions test.</summary>
	public partial class ExpressionTests
	{
		/// <summary>A test class.</summary>
		private static class TestClass
		{
			/// <summary>Gets the static property.</summary>
			/// <value>The static property.</value>
			public static int StaticProperty { get { return 12; } }

			/// <summary>The static field.</summary>
			public static int _staticField = 12;
		}

		/// <summary>A test class.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		private class TestClass<T>
		{
			/// <summary>Static method.</summary>
			/// <param name="value">The value.</param>
			/// <returns>A T.</returns>
			public static T StaticMethod(T value)
			{
				return value;
			}

			/// <summary>Gets or sets the property.</summary>
			/// <value>The property.</value>
			public T Property { get; set; }

			/// <summary>The field.</summary>
			public T _field;

			/// <summary>Gets the mehtod.</summary>
			/// <returns>A T.</returns>
			public T Mehtod()
			{
				return _field;
			}

			/// <summary>
			/// Initializes a new instance of the
			/// NServiceKit.OrmLite.Tests.Expression.PrimaryExpressionsTest.TestClass&lt;T&gt; class.
			/// </summary>
			/// <param name="value">The value.</param>
			public TestClass(T value)
			{
				Property = value;
				_field = value;
			}
		}

		/// <summary>A test structure.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		private struct TestStruct<T>
		{
			/// <summary>Gets the property.</summary>
			/// <value>The property.</value>
			public T Property { get { return _field; } }

			/// <summary>The field.</summary>
			public T _field;

			/// <summary>Gets the mehtod.</summary>
			/// <returns>A T.</returns>
			public T Mehtod()
			{
				return _field;
			}

			/// <summary>
			/// Initializes a new instance of the NServiceKit.OrmLite.Tests.Expression.PrimaryExpressionsTest
			/// class.
			/// </summary>
			/// <param name="value">The value.</param>
			public TestStruct(T value)
			{
				_field = value;
			}
		}

		#region int
		/// <summary>Can select int property expression.</summary>
		[Test]
		public void Can_select_int_property_expression()
		{
			var tmp = new TestClass<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp.Property);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select int field expression.</summary>
		[Test]
		public void Can_select_int_field_expression()
		{
			var tmp = new TestClass<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp._field);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select int method expression.</summary>
		[Test]
		public void Can_select_int_method_expression()
		{
			var tmp = new TestClass<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp.Mehtod());

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select static int property expression.</summary>
		[Test]
		public void Can_select_static_int_property_expression()
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
				var actual = conn.Select<TestType>(q => q.IntColumn == TestClass.StaticProperty);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select static int field expression.</summary>
		[Test]
		public void Can_select_static_int_field_expression()
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
				var actual = conn.Select<TestType>(q => q.IntColumn == TestClass._staticField);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select static int method expression.</summary>
		[Test]
		public void Can_select_static_int_method_expression()
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
				var actual = conn.Select<TestType>(q => q.IntColumn == TestClass<int>.StaticMethod(12));

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select int new expression.</summary>
		[Test]
		public void Can_select_int_new_expression()
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
				var actual = conn.Select<TestType>(q => q.IntColumn == new TestClass<int>(12).Property);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select structure int field expression.</summary>
		[Test]
		public void Can_select_struct_int_field_expression()
		{
			var tmp = new TestStruct<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp._field);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select structure int property expression.</summary>
		[Test]
		public void Can_select_struct_int_property_expression()
		{
			var tmp = new TestStruct<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp.Property);

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select structure int method expression.</summary>
		[Test]
		public void Can_select_struct_int_method_expression()
		{
			var tmp = new TestStruct<int>(12);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.IntColumn == tmp.Mehtod());

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}

		#endregion int


		#region bool
		/// <summary>Can select bool property expression.</summary>
		[Test]
		public void Can_select_bool_property_expression()
		{
			var tmp = new TestClass<bool>(false);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == tmp.Property);

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 1);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select bool field expression.</summary>
		[Test]
		public void Can_select_bool_field_expression()
		{
			var tmp = new TestClass<bool>(false);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == tmp._field);

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 1);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select bool method expression.</summary>
		[Test]
		public void Can_select_bool_method_expression()
		{
			var tmp = new TestClass<bool>(false);

			var expected = new TestType()
			{
				IntColumn = 12,
				BoolColumn = false,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.Select<TestType>(q => q.BoolColumn == tmp.Mehtod());

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 1);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select static bool method expression.</summary>
		[Test]
		public void Can_select_static_bool_method_expression()
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
				var actual = conn.Select<TestType>(q => q.BoolColumn == TestClass<bool>.StaticMethod(false));

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 1);
				CollectionAssert.Contains(actual, expected);
			}
		}

		/// <summary>Can select bool new expression.</summary>
		[Test]
		public void Can_select_bool_new_expression()
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
				var actual = conn.Select<TestType>(q => q.BoolColumn == new TestClass<bool>(false).Property);

				Assert.IsNotNull(actual);
				Assert.Greater(actual.Count(), 1);
				CollectionAssert.Contains(actual, expected);
			}
		}

		#endregion bool
	}
}