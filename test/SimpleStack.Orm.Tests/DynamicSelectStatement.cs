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
                var r = conn.Select("TestType2",
                                    x => x.Select<int>("id",
                                                       y => Sql.As(Sql.Max(y), "maxintcol")));

                var results = conn.Select("TestType2",
                                          x => x.Select("id", "textcol")
                                                .Where("textcol", (string y) => y.Substring(0, 1) == "a")
                                                .And("id", (int y) => y > 10)
                                                .Limit(1, 1));
            }
        }
    }
}