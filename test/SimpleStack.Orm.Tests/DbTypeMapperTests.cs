using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        class Simple
        {
            public string Test { get; set; }
        }
        
        class SimplePrecision
        {
            public decimal Test { get; set; }
        }
        
        [Test]
        public virtual void CanSetDefaultTypeMapperLength()
        {
            var bck = _connectionFactory.DialectProvider.TypesMapper.DefaultStringLength;
            _connectionFactory.DialectProvider.TypesMapper.DefaultStringLength = 12;
            using (var db = OpenDbConnection())
            {
                db.CreateTable<Simple>(true);
                var table = db.GetTables().FirstOrDefault(x => x.Name.ToLower() == "simple");
                var column = db.GetTableColumns(table.Name, table.SchemaName).FirstOrDefault();
                Assert.AreEqual(12, column.Length);
                db.DropTableIfExists<Simple>();
            }

            _connectionFactory.DialectProvider.TypesMapper.DefaultStringLength = bck;
        }
        
        
        [Test]
        public virtual void CanSetDefaultTypeMapperPrecisionAndScale()
        {
            _connectionFactory.DialectProvider.TypesMapper.DefaultPrecision = 8;
            _connectionFactory.DialectProvider.TypesMapper.DefaultScale = 4;
            using (var db = OpenDbConnection())
            {
                db.CreateTable<SimplePrecision>(true);
                var table = db.GetTables().FirstOrDefault(x => x.Name.ToLower() == "simpleprecision");
                var column = db.GetTableColumns(table.Name, table.SchemaName).FirstOrDefault();
                Assert.AreEqual(8, column.Precision);
                Assert.AreEqual(4, column.Scale);
                db.DropTableIfExists<SimplePrecision>();
            }
        }
    }
}