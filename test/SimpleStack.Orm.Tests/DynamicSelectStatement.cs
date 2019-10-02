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
        [Test]
        public void TestDynamicQuery()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var r2 = conn.Select("TestType2", x => x.Where<int>("CASE WHEN 1 > 2 THEN 33 ELSE 44 END", y => y == 1 ));

                var r = conn.Select("TestType2",
                                    x => x.Select<int>(conn.DialectProvider.GetQuotedColumnName("id"),
                                                       y => Sql.As(Sql.Max(y), "maxintcol")));

                var results = conn.Select("TestType2",
                                          x => x.Select(conn.DialectProvider.GetQuotedColumnName("id"), conn.DialectProvider.GetQuotedColumnName("textcol"))
                                                .Where(conn.DialectProvider.GetQuotedColumnName("textcol"), (string y) => y.Substring(0, 1) == "a")
                                                .And(conn.DialectProvider.GetQuotedColumnName("id"), (int y) => y > 10)
                                                .Limit(1, 1));

                var rrr = conn.Count("TestType2",x =>
                {
                    x.And(conn.DialectProvider.GetQuotedColumnName("textcol"), (string y) => y.Substring(0,1) == "a");
                });


                Assert.AreEqual(2, rrr);
            }
        }
    }
}