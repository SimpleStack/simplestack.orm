﻿using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    /// <summary>A string function tests.</summary>
    public partial class ExpressionTests
    {
        /// <summary>Can select using contains.</summary>
        [Test]
        public void Can_select_using_contains()
        {
            var stringVal = "stringValue";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = stringVal
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.Contains("gVal"));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);

                actual = conn.Select<TestType>(q => q.StringColumn.Contains("TRINGVALU"));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using contains with backtick in string.</summary>
        [Test]
        public void Can_select_using_contains_with_backtick_in_string()
        {
            var stringVal = "string`ContainingAQuote";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = stringVal
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.Contains("ng`Co"));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using contains with double quote in string.</summary>
        [Test]
        public void Can_select_using_contains_with_double_quote_in_string()
        {
            var stringVal = "string\"ContainingAQuote";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = stringVal
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.Contains("ng\"Con"));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using contains with quote in string.</summary>
        [Test]
        public void Can_select_using_contains_with_quote_in_string()
        {
            var stringVal = "string'ContainingAQuote";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = stringVal
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.Contains("g'Co"));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using ends with.</summary>
        [Test]
        public void Can_select_using_endsWith()
        {
            var postfix = "postfix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = "asdfasdfasdf" + postfix
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.EndsWith(postfix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using ends with backtick in string.</summary>
        [Test]
        public void Can_select_using_endsWith_with_backtick_in_string()
        {
            var postfix = "postfix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = "asdfasd`fasdf" + postfix
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.EndsWith(postfix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using ends with double quote in string.</summary>
        [Test]
        public void Can_select_using_endsWith_with_double_quote_in_string()
        {
            var postfix = "postfix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = "asdfasd\"fasdf" + postfix
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.EndsWith(postfix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using ends with quote in string.</summary>
        [Test]
        public void Can_select_using_endsWith_with_quote_in_string()
        {
            var postfix = "postfix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = "asdfasd'fasdf" + postfix
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.EndsWith(postfix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using starts with.</summary>
        [Test]
        public void Can_select_using_startsWith()
        {
            var prefix = "prefix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = prefix + "asdfasdfasdf"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.StartsWith(prefix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using starts with backtick in string.</summary>
        [Test]
        public void Can_select_using_startsWith_with_backtick_in_string()
        {
            var prefix = "prefix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = prefix + "`asdfasdfasdf"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.StartsWith(prefix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using starts with double quote in string.</summary>
        [Test]
        public void Can_select_using_startsWith_with_double_quote_in_string()
        {
            var prefix = "prefix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = prefix + "\"asdfasdfasdf"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.StartsWith(prefix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }

        /// <summary>Can select using starts with quote in string.</summary>
        [Test]
        public void Can_select_using_startsWith_with_quote_in_string()
        {
            var prefix = "prefix";

            var expected = new TestType
            {
                IntColumn = 7,
                BoolColumn = true,
                StringColumn = prefix + "'asdfasdfasdf"
            };

            EstablishContext(10, expected);
            using (var conn = OpenDbConnection())
            {
                var actual = conn.Select<TestType>(q => q.StringColumn.StartsWith(prefix));

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
                CollectionAssert.Contains(actual, expected);
            }
        }
    }
}