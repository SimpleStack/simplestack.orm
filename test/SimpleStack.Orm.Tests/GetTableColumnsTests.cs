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
        public void CanGetColumns()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<Member>(true);
                AddMembers(db);
                var columns = db.GetTableColumnsInformation("member", "public");
                Assert.Equals(columns.Count(), 11);
            }
        }
    }
}
