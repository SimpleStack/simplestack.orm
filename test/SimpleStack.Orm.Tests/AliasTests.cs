using System.Linq;
using Dapper;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
    [TestFixture]
    public partial class ExpressionTests
    {
        public class AliasType
        {
            [Alias("Alias_Name")] public string ColumnWithAlias { get; set; }

            public string ColumnWithoutAlias { get; set; }
        }

        public class Bar
        {
            [PrimaryKey] public int Id { get; set; }

            [Alias("Test")] public string Foo { get; set; }
        }

        [Test]
        public void Can_create_table_with_alias()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<AliasType>(true);

                var result = conn.Query("select Alias_Name from AliasType");
            }
        }

        [Test]
        public void Can_select_all_from_table_with_alias()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<AliasType>(true);

                conn.Insert(new AliasType {ColumnWithAlias = "CCC", ColumnWithoutAlias = "FFF"});
                conn.Insert(new AliasType {ColumnWithAlias = "AAA", ColumnWithoutAlias = "DDD"});
                conn.Insert(new AliasType {ColumnWithAlias = "BBB", ColumnWithoutAlias = "EEE"});

                var result = conn.Select<AliasType>(x => x.OrderBy(y => y.ColumnWithAlias)).ToArray();

                Assert.AreEqual("AAA", result[0].ColumnWithAlias);
                Assert.AreEqual("DDD", result[0].ColumnWithoutAlias);
                Assert.AreEqual("BBB", result[1].ColumnWithAlias);
                Assert.AreEqual("EEE", result[1].ColumnWithoutAlias);
                Assert.AreEqual("CCC", result[2].ColumnWithAlias);
                Assert.AreEqual("FFF", result[2].ColumnWithoutAlias);
            }
        }

        [Test]
        public void Can_select_partial_from_table_with_alias()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<AliasType>(true);

                conn.Insert(new AliasType {ColumnWithAlias = "CCC", ColumnWithoutAlias = "FFF"});
                conn.Insert(new AliasType {ColumnWithAlias = "AAA", ColumnWithoutAlias = "DDD"});
                conn.Insert(new AliasType {ColumnWithAlias = "BBB", ColumnWithoutAlias = "EEE"});

                var result = conn.Select<AliasType>(x =>
                {
                    x.OrderBy(y => y.ColumnWithAlias);
                    x.Select(y => y.ColumnWithAlias);
                }).ToArray();

                //SELECT "alias_name" FROM "aliastype"ORDER BY "alias_name" ASC
                Assert.AreEqual("AAA", result[0].ColumnWithAlias);
                Assert.IsNull(result[0].ColumnWithoutAlias);
                Assert.AreEqual("BBB", result[1].ColumnWithAlias);
                Assert.IsNull(result[1].ColumnWithoutAlias);
                Assert.AreEqual("CCC", result[2].ColumnWithAlias);
                Assert.IsNull(result[2].ColumnWithoutAlias);
            }
        }

        [Test]
        public void TestGetObjectByPropertyWithAlias()
        {
            using (var connection = OpenDbConnection())
            {
                connection.CreateTable<Bar>(true);
                connection.Insert(new Bar {Id = 1, Foo = "test"});
                var bar = connection.FirstOrDefault<Bar>(x => x.Foo == "test");
                Assert.IsNotNull(bar);
                Assert.AreEqual(1, bar.Id);
                Assert.AreEqual("test", bar.Foo);
                connection.Close();
            }
        }

        [Test]
        public void TestPropertyWithAliasCorrectlyMappedInClass()
        {
            using (var connection = OpenDbConnection())
            {
                connection.CreateTable<Bar>(true);
                connection.Insert(new Bar {Id = 1, Foo = "test"});
                var bar = connection.FirstOrDefault<Bar>(x => x.Id == 1);
                Assert.IsNotNull(bar);
                Assert.AreEqual(1, bar.Id);
                Assert.AreEqual("test", bar.Foo);
                connection.Close();
            }
        }
    }
}