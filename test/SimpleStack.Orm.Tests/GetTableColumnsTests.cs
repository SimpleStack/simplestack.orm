using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
    [Schema("schema1")]
    class TestUnique1
    {
        [PrimaryKey]
        public int Id { get; set; }

        [Index(Unique = true)]
        public string TextCol { get; set; }
        
        public bool BoolCol { get; set; }
    }

    [Schema("schema2")]
    class TestUnique2
    {
        [PrimaryKey]
        public int Id { get; set; }
        
        [Index(Unique = false)]
        public string TextCol { get; set; }
        
        public bool BoolCol { get; set; }
    }
    
    [Schema("schema3")]
    class TestUnique3
    {
        [PrimaryKey]
        public int Id { get; set; }
        
        public string TextCol { get; set; }
        
        public bool BoolCol { get; set; }
    }
    
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
                Assert.True(columns[0].Unique);
                Assert.AreEqual(DbType.Int32, columns[0].DbType);
                
                Assert.AreEqual("textcol", columns[1].Name.ToLower());
                Assert.AreEqual(false, columns[1].PrimaryKey);
                Assert.True(columns[1].Unique);
                Assert.AreEqual(DbType.String, columns[1].DbType);

                Assert.AreEqual("boolcol", columns[2].Name.ToLower());
                Assert.AreEqual(false, columns[2].PrimaryKey);
                Assert.False(columns[2].Unique);
                Assert.AreEqual(DbType.Boolean, columns[2].DbType);

                Assert.AreEqual("datecol", columns[3].Name.ToLower());
                Assert.AreEqual(false, columns[3].PrimaryKey);
                Assert.False(columns[3].Unique);
                Assert.AreEqual(DbType.DateTime, columns[3].DbType);

                Assert.AreEqual("enumcol", columns[4].Name.ToLower());
                Assert.AreEqual(false, columns[4].PrimaryKey);
                Assert.False(columns[4].Unique);
                Assert.AreEqual(DbType.Int32, columns[4].DbType);
                
                Assert.AreEqual("guidcol", columns[5].Name.ToLower());
                Assert.AreEqual(false, columns[5].PrimaryKey);
                Assert.False(columns[5].Unique);
                Assert.AreEqual(DbType.Guid, columns[5].DbType);

                Assert.AreEqual("complexobjcol", columns[6].Name.ToLower());
                Assert.AreEqual(false, columns[6].PrimaryKey);
                Assert.False(columns[6].Unique);
                Assert.AreEqual(DbType.Binary, columns[6].DbType);

                Assert.AreEqual("longcol", columns[7].Name.ToLower());
                Assert.AreEqual(false, columns[7].PrimaryKey);
                Assert.False(columns[7].Unique);
                Assert.AreEqual(DbType.Int64, columns[7].DbType);
            }
        }
        
        [Test]
        public void TestIndexUniqueFlag()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateSchemaIfNotExists("schema1");
                db.CreateSchemaIfNotExists("schema2");
                db.CreateSchemaIfNotExists("schema3");
                
                db.CreateTable<TestUnique1>(true);
                db.CreateTable<TestUnique2>(true);
                db.CreateTable<TestUnique3>(true);
                
                var columnsInTable1 = db.GetTableColumnsInformation("TestUnique1", "schema1").ToArray();
                var columnsInTable2 = db.GetTableColumnsInformation("TestUnique2", "schema2").ToArray();
                var columnsInTable3 = db.GetTableColumnsInformation("TestUnique3", "schema3").ToArray();

                Assert.AreEqual("textcol", columnsInTable1[1].Name.ToLower());
                Assert.True(columnsInTable1[1].Unique);

                Assert.AreEqual("textcol", columnsInTable2[1].Name.ToLower());
                Assert.False(columnsInTable2[1].Unique);

                Assert.AreEqual("textcol", columnsInTable3[1].Name.ToLower());
                Assert.False(columnsInTable3[1].Unique);
            }
        }
    }
}
