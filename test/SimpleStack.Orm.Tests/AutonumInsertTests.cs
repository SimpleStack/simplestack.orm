using NUnit.Framework;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
    [TestFixture]
    public partial class ExpressionTests
    {
        public class ModeWithAutoIncrement
        {
            /// <summary>Gets or sets the identifier.</summary>
            /// <value>The identifier.</value>
            [AutoIncrement]
            [PrimaryKey]
            public int Id { get; set; }

            /// <summary>Gets or sets the name.</summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }
        
        public class ModeWithoutAutoIncrement
        {
            /// <summary>Gets or sets the identifier.</summary>
            /// <value>The identifier.</value>
            [PrimaryKey]
            public int Id { get; set; }

            /// <summary>Gets or sets the name.</summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }

        [Test]
        public void InsertAndRetrieveIdentity()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<ModeWithAutoIncrement>(true);

                var identity = conn.Insert(new ModeWithAutoIncrement{Name = "hello"});
                var identity2 = conn.Insert(new ModeWithAutoIncrement{Name = "hello"});
                
                Assert.NotNull(identity);
                Assert.False(identity == identity2);
            }
        }
        
        [Test]
        public void InsertWithoutAutoIncrementAndRetrieveIdentity()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<ModeWithoutAutoIncrement>(true);

                var identity = conn.Insert(new ModeWithoutAutoIncrement{Id=10, Name = "hello"});
                
                Assert.AreEqual(0,identity);
            }
        }
        
    }
}