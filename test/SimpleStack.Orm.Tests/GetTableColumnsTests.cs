using System.Data;
using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        [Test]
        public virtual void CanGetColumns()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TestType2>(true);
                var columns = db.GetTableColumnsInformation("TestType2").ToArray();
                Assert.AreEqual(9, columns.Length);

                Assert.AreEqual("id", columns[0].Name.ToLower());
                Assert.True(columns[0].PrimaryKey);
                Assert.AreEqual(DbType.Int32, columns[0].DbType);
                Assert.False(columns[0].Nullable);

                Assert.AreEqual("textcol", columns[1].Name.ToLower());
                Assert.AreEqual(false, columns[1].PrimaryKey);
                Assert.AreEqual(DbType.String, columns[1].DbType);
                Assert.True(columns[1].Nullable);

                Assert.AreEqual("boolcol", columns[2].Name.ToLower());
                Assert.AreEqual(false, columns[2].PrimaryKey);
                Assert.AreEqual(DbType.Boolean, columns[2].DbType);
                Assert.False(columns[2].Nullable);
                
                Assert.AreEqual("datecol", columns[3].Name.ToLower());
                Assert.AreEqual(false, columns[3].PrimaryKey);
                Assert.AreEqual(DbType.DateTime, columns[3].DbType);
                Assert.False(columns[3].Nullable);
                
                Assert.AreEqual("nullabledatecol", columns[4].Name.ToLower());
                Assert.False(columns[4].PrimaryKey);
                Assert.AreEqual(DbType.DateTime, columns[4].DbType);
                Assert.True(columns[4].Nullable);

                Assert.AreEqual("enumcol", columns[5].Name.ToLower());
                Assert.AreEqual(false, columns[5].PrimaryKey);
                Assert.AreEqual(DbType.Int32, columns[5].DbType);

                Assert.AreEqual("guidcol", columns[6].Name.ToLower());
                Assert.AreEqual(false, columns[6].PrimaryKey);
                Assert.AreEqual(DbType.Guid, columns[6].DbType);

                Assert.AreEqual("complexobjcol", columns[7].Name.ToLower());
                Assert.AreEqual(false, columns[7].PrimaryKey);
                Assert.AreEqual(DbType.Binary, columns[7].DbType);

                Assert.AreEqual("longcol", columns[8].Name.ToLower());
                Assert.AreEqual(false, columns[8].PrimaryKey);
                Assert.AreEqual(DbType.Int64, columns[8].DbType);
            }
        }
    }
}