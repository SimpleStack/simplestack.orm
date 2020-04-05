﻿using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    /// <summary>An equality expressions test.</summary>
    public partial class ExpressionTests
    {
        /// <summary>Can select equals bool method expression.</summary>
        [Test]
        public void Can_select_equals_bool_method_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == GetValue(true));

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals constant bool expression.</summary>
        [Test]
        public void Can_select_equals_constant_bool_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            // ReSharper disable RedundantBoolCompare
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == true);
                // ReSharper restore RedundantBoolCompare

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals constant bool expression 2.</summary>
        [Test]
        public void Can_select_equals_constant_bool_expression2()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            // ReSharper disable RedundantBoolCompare
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn);
                // ReSharper restore RedundantBoolCompare

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals constant int expression.</summary>
        [Test]
        public void Can_select_equals_constant_int_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == 3);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals int method expression.</summary>
        [Test]
        public void Can_select_equals_int_method_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == GetValue(3));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals null espression.</summary>
        [Test]
        public void Can_select_equals_null_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = null
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn != null);

                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Count(), 10);
                CollectionAssert.DoesNotContain(actual, expected);
            }
        }

        /// <summary>Can select equals variable bool expression.</summary>
        [Test]
        public void Can_select_equals_variable_bool_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var columnValue = true;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.BoolColumn == columnValue);

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select equals variable int expression.</summary>
        [Test]
        public void Can_select_equals_variable_int_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var columnValue = 3;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = columnValue,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn == columnValue);

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select not equals constant int expression.</summary>
        [Test]
        public void Can_select_not_equals_constant_int_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn != 3);

                Assert.IsNotNull(actual);
                Assert.AreEqual(10, actual.Count());
                CollectionAssert.DoesNotContain(actual, expected);
            }
        }

        /// <summary>Can select not equals int method expression.</summary>
        [Test]
        public void Can_select_not_equals_int_method_expression()
        {
            var expected = new TestType
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn != GetValue(3));

                Assert.IsNotNull(actual);
                Assert.AreEqual(10, actual.Count());
                CollectionAssert.DoesNotContain(actual, expected);
            }
        }

        /// <summary>Can select not equals null espression.</summary>
        [Test]
        public void Can_select_not_equals_null_expression()
        {
            var expected = new TestType
            {
                IntColumn = 12,
                BoolColumn = false,
                StringColumn = null
            };

            using (var conn = OpenDbConnection())
            {
                conn.Insert(expected);
                expected.StringColumn = "hello";
                conn.Insert(expected);

                var actual = conn.Select<TestType>(q => q.StringColumn != null);

                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Count(), 1);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select not equals variable int expression.</summary>
        [Test]
        public void Can_select_not_equals_variable_int_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var columnValue = 3;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType
            {
                IntColumn = columnValue,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.IntColumn != columnValue);

                Assert.IsNotNull(actual);
                Assert.AreEqual(10, actual.Count());
                CollectionAssert.DoesNotContain(actual, expected);
            }
        }

        // Assume not equal works ;-)
    }
}