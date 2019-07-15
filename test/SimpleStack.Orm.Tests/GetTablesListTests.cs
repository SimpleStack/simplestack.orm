using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        [Test]
        public void GetExistingTablesList()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<CompositeMember>(true);
                db.CreateTable<TestType2>(true);

                var tables = db.GetTablesInformation().ToArray();
                Assert.True(tables.Any(x => x.Name.ToLower() == "testtype2"));
                Assert.True(tables.All(x => !string.IsNullOrWhiteSpace(x.SchemaName)));
            }
        }
    }
}
