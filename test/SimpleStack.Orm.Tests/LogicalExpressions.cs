using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    /// <summary>A logical expressions test.</summary>
    public partial class ExpressionTests
    {
        /// <summary>Can select logical and method expression.</summary>
        [Test]
        public void Can_select_logical_and_method_expression()
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
                var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) & GetValue(false)));

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select logical and variable expression.</summary>
        [Test]
        public void Can_select_logical_and_variable_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var a = true;
            var b = false;
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
                var actual = conn.Select<TestType>(q => q.BoolColumn == (a & b));

                Assert.IsNotNull(actual);
                Assert.Greater(actual.Count(), 0);
                CollectionAssert.Contains(actual, expected);
            }
        }

        [Test]
        public void Can_Select_Logical_Bitwise_and_int_expression()
        {
            var p = 3;

            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType {Id = 1, BoolColumn = false, IntColumn = 1, StringColumn = "test"});
                conn.Insert(new TestType {Id = 2, BoolColumn = false, IntColumn = 2, StringColumn = "test"});
                conn.Insert(new TestType {Id = 3, BoolColumn = false, IntColumn = 4, StringColumn = "test"});

                var actual = conn.Select<TestType>(x => (x.IntColumn & p) != 0);
                Assert.IsNotNull(actual);
                Assert.AreEqual(2, actual.Count());
                Assert.True(actual.Any(x => x.Id == 1));
                Assert.True(actual.Any(x => x.Id == 2));
            }
        }

        [Test]
        public virtual void Can_Select_Logical_Bitwise_leftshift_int_expression()
        {
            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType {Id = 1, BoolColumn = false, IntColumn = 1, StringColumn = "test"});
                conn.Insert(new TestType {Id = 2, BoolColumn = false, IntColumn = 2, StringColumn = "test"});
                conn.Insert(new TestType {Id = 3, BoolColumn = false, IntColumn = 4, StringColumn = "test"});

                var actual = conn.Select<TestType>(x => x.IntColumn << 2 == 4);
                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                Assert.True(actual.Any(x => x.Id == 1));
            }
        }

        [Test]
        public void Can_Select_Logical_Bitwise_or_int_expression()
        {
            var p = 3;

            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType {Id = 1, BoolColumn = false, IntColumn = 1, StringColumn = "test"});
                conn.Insert(new TestType {Id = 2, BoolColumn = false, IntColumn = 2, StringColumn = "test"});
                conn.Insert(new TestType {Id = 3, BoolColumn = false, IntColumn = 4, StringColumn = "test"});

                var actual = conn.Select<TestType>(x => (x.IntColumn | p) == p);
                Assert.IsNotNull(actual);
                Assert.AreEqual(2, actual.Count());
                Assert.True(actual.Any(x => x.Id == 1));
                Assert.True(actual.Any(x => x.Id == 2));
            }
        }

        [Test]
        public virtual void Can_Select_Logical_Bitwise_rightshift_int_expression()
        {
            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType {Id = 1, BoolColumn = false, IntColumn = 1, StringColumn = "test"});
                conn.Insert(new TestType {Id = 2, BoolColumn = false, IntColumn = 2, StringColumn = "test"});
                conn.Insert(new TestType {Id = 3, BoolColumn = false, IntColumn = 4, StringColumn = "test"});

                var actual = conn.Select<TestType>(x => x.IntColumn >> 2 == 1);
                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                Assert.True(actual.Any(x => x.Id == 3));
            }
        }

        [Test]
        public virtual void Can_Select_Logical_Bitwise_xor_int_expression()
        {
            var p = 3;

            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType {Id = 1, BoolColumn = false, IntColumn = 1, StringColumn = "test"});
                conn.Insert(new TestType {Id = 2, BoolColumn = false, IntColumn = 2, StringColumn = "test"});
                conn.Insert(new TestType {Id = 3, BoolColumn = false, IntColumn = 4, StringColumn = "test"});

                var actual = conn.Select<TestType>(x => (x.IntColumn ^ p) == 7);
                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                Assert.True(actual.Any(x => x.Id == 3));
            }
        }

        /// <summary>Can select logical or method expression.</summary>
        [Test]
        public void Can_select_logical_or_method_expression()
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
                var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) | GetValue(false)));

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

            var expected = new TestType
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

        /// <summary>Can select logical exclusive-or method expression.</summary>
        [Test]
        public void Can_select_logical_xor_method_expression()
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
                var actual = conn.Select<TestType>(q => q.BoolColumn == (GetValue(true) ^ GetValue(false)));

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

            var expected = new TestType
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

        // Unlikely 
        // OpenDbConnection().Select<TestType>(q => q.BoolColumn == (true & false));
    }
}