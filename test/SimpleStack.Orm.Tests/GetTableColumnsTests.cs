using System.Data;
using System.Linq;
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
                Assert.AreEqual(8, columns.Length);

                Assert.AreEqual("id", columns[0].Name.ToLower());
                Assert.AreEqual(true, columns[0].PrimaryKey);
                Assert.AreEqual(DbType.Int32, columns[0].DbType);

                Assert.AreEqual("textcol", columns[1].Name.ToLower());
                Assert.AreEqual(false, columns[1].PrimaryKey);
                Assert.AreEqual(DbType.String, columns[1].DbType);

                Assert.AreEqual("boolcol", columns[2].Name.ToLower());
                Assert.AreEqual(false, columns[2].PrimaryKey);
                Assert.AreEqual(DbType.Boolean, columns[2].DbType);

                Assert.AreEqual("datecol", columns[3].Name.ToLower());
                Assert.AreEqual(false, columns[3].PrimaryKey);
                Assert.AreEqual(DbType.DateTime, columns[3].DbType);

                Assert.AreEqual("enumcol", columns[4].Name.ToLower());
                Assert.AreEqual(false, columns[4].PrimaryKey);
                Assert.AreEqual(DbType.Int32, columns[4].DbType);

                Assert.AreEqual("guidcol", columns[5].Name.ToLower());
                Assert.AreEqual(false, columns[5].PrimaryKey);
                Assert.AreEqual(DbType.Guid, columns[5].DbType);

                Assert.AreEqual("complexobjcol", columns[6].Name.ToLower());
                Assert.AreEqual(false, columns[6].PrimaryKey);
                Assert.AreEqual(DbType.Binary, columns[6].DbType);

                Assert.AreEqual("longcol", columns[7].Name.ToLower());
                Assert.AreEqual(false, columns[7].PrimaryKey);
                Assert.AreEqual(DbType.Int64, columns[7].DbType);
            }
        }
    }
}