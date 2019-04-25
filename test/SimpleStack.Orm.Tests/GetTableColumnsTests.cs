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
                db.CreateTable<TestType2>(true);
                var columns = db.GetTableColumnsInformation("TestType2").ToArray();
                Assert.AreEqual(6, columns.Length);
                Assert.AreEqual("id", columns[0].Name.ToLower());
                Assert.AreEqual(true, columns[0].PrimaryKey);
                Assert.AreEqual("textcol", columns[1].Name.ToLower());
                Assert.AreEqual(false, columns[1].PrimaryKey);
                Assert.AreEqual("boolcol", columns[2].Name.ToLower());
                Assert.AreEqual(false, columns[2].PrimaryKey);
                Assert.AreEqual("datecol", columns[3].Name.ToLower());
                Assert.AreEqual(false, columns[3].PrimaryKey);
                Assert.AreEqual("enumcol", columns[4].Name.ToLower());
                Assert.AreEqual(false, columns[4].PrimaryKey);
                Assert.AreEqual("complexobjcol", columns[5].Name.ToLower());
                Assert.AreEqual(false, columns[5].PrimaryKey);
            }
        }
    }
}
