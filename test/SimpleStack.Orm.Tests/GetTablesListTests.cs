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
                AddMembers(db);
                db.CreateTable<CompositeMember>(true);
                Assert.Equals(db.GetTablesInformation("postgres", "public").Count(), 2);
            }
        }
    }
}
