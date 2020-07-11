using NUnit.Framework;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Sqlite;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        [Alias("tests")]
        public class TestReservedWordAliasClass
        {
            [Alias("id")]
            public int Id { get; set; }

            [Alias("order_test")]
            public int Order { get; set; }
        }

        [Test]
        public void Can_Select_With_Reserved_Word_Alias()
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                connection.CreateTable<TestReservedWordAliasClass>(true);
                var tmp = connection.Select<TestReservedWordAliasClass>();
                connection.DropTableIfExists<TestReservedWordAliasClass>();
            }
        }
    }
}