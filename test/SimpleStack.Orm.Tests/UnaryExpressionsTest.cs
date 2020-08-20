using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    /// <summary>An unary expressions test.</summary>
    public partial class ExpressionTests
    {
        /// <summary>Can select unary cast method expression.</summary>
        [Test]
        public void Can_select_unary_cast_method_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == (int) GetValue((object) 12));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary cast variable expression.</summary>
        [Test]
        public void Can_select_unary_cast_variable_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            object intVal = 12;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == (int) intVal);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>
        ///     Can select unary cast variable with explicit userdefined type conversion expression.
        /// </summary>
        [Test]
        public void Can_select_unary_cast_variable_with_explicit_userdefined_type_conversion_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var intVal = 12;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);

            var value = new IntWrapper(intVal);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == (int) value);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>
        ///     Can select unary cast variable with implicit userdefined type conversion expression.
        /// </summary>
        [Test]
        public void Can_select_unary_cast_variable_with_implicit_userdefined_type_conversion_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var intVal = 12;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);

            var value = new IntWrapper(intVal);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == value);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary minus constant expression.</summary>
        [Test]
        public void Can_select_unary_minus_constant_expression()
        {
            var expected = new TestType
            {
                IntColumn = -12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == -12);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary minus variable expression.</summary>
        [Test]
        public void Can_select_unary_minus_variable_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var intVal = -12;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = -12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == intVal);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary not constant expression.</summary>
        [Test]
        public void Can_select_unary_not_constant_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == !true);

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary not constant expression 2.</summary>
        [Test]
        public void Can_select_unary_not_constant_expression2()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => !q.BoolColumn);

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary not method expression.</summary>
        [Test]
        public void Can_select_unary_not_method_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == !GetValue(true));

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary not variable expression.</summary>
        [Test]
        public void Can_select_unary_not_variable_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var boolVal = true;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == !boolVal);

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary plus constant expression.</summary>
        [Test]
        public void Can_select_unary_plus_constant_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == +12);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select unary plus variable expression.</summary>
        [Test]
        public void Can_select_unary_plus_variable_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var intVal = +12;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = true,
                StringColumn = "test"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == intVal);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }
    }

    /// <summary>An int wrapper.</summary>
    public struct IntWrapper
    {
        /// <summary>The identifier.</summary>
        private readonly int _id;

        /// <summary>Initializes a new instance of the UnaryExpressionsTest class.</summary>
        /// <param name="projectId">Identifier for the project.</param>
        public IntWrapper(int projectId)
        {
            _id = projectId;
        }

        /// <summary>IntWrapper casting operator.</summary>
        /// <param name="value">The value.</param>
        public static explicit operator IntWrapper(int value)
        {
            return new IntWrapper(value);
        }

        /// <summary>int casting operator.</summary>
        /// <param name="value">The value.</param>
        public static implicit operator int(IntWrapper value)
        {
            return value._id;
        }
    }
}