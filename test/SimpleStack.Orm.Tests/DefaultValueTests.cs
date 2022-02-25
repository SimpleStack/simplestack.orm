using Dapper;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
    [TestFixture]
    public partial class ExpressionTests
    {
        public class ModeWithDefaultValue
        {
            /// <summary>Gets or sets the identifier.</summary>
            /// <value>The identifier.</value>
            [AutoIncrement]
            [PrimaryKey]
            public int Id { get; set; }

            /// <summary>Gets or sets the name.</summary>
            /// <value>The name.</value>
            [Default("John")]
            public string Name { get; set; }
        }

        public class ModeWithoutDefaultValue
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

        [Test]
        public void InsertWithDefaultValue()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<ModeWithDefaultValue>(true);

                var identity = conn.Insert(new ModeWithDefaultValue());
                var model = conn.FirstOrDefault<ModeWithDefaultValue>(x => x.Id == identity);
                
                Assert.AreEqual("John",model.Name);
            }
        }
        
        [Test]
        public void InsertWithoutDefaultValue()
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<ModeWithoutDefaultValue>(true);

                var identity = conn.Insert(new ModeWithoutDefaultValue());
                var model = conn.FirstOrDefault<ModeWithoutDefaultValue>(x => x.Id == identity);
                
                Assert.IsNull(model.Name);
            }
        }
    }
}