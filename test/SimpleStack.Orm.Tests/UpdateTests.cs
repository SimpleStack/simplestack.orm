using System;
using System.Linq;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;

//using SimpleStack.Orm.MySQL;

namespace SimpleStack.Orm.Tests
{
    [TestFixture]
    public partial class ExpressionTests
    {
        /// <summary>A model with fields of different types.</summary>
        public class ModelWithFieldsOfDifferentTypes
        {
            /// <summary>Gets or sets the identifier.</summary>
            /// <value>The identifier.</value>
            [AutoIncrement]
            [PrimaryKey]
            public int Id { get; set; }

            /// <summary>Gets or sets the name.</summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>Gets or sets the identifier of the long.</summary>
            /// <value>The identifier of the long.</value>
            public long LongId { get; set; }

            /// <summary>Gets or sets a value indicating whether the. </summary>
            /// <value>true if , false if not.</value>
            public bool Bool { get; set; }

            /// <summary>Gets or sets the date time.</summary>
            /// <value>The date time.</value>
            public DateTime DateTime { get; set; }

            /// <summary>Gets or sets the double.</summary>
            /// <value>The double.</value>
            public double Double { get; set; }

            public Guid Guid { get; set; }

            /// <summary>Creates a new ModelWithFieldsOfDifferentTypes.</summary>
            /// <param name="id">The identifier.</param>
            /// <returns>The ModelWithFieldsOfDifferentTypes.</returns>
            public static ModelWithFieldsOfDifferentTypes Create(int id)
            {
                var row = new ModelWithFieldsOfDifferentTypes
                {
                    Id = id,
                    Bool = id % 2 == 0,
                    DateTime = DateTime.Now.AddDays(id),
                    Double = 1.11d + id,
                    LongId = 999 + id,
                    Name = "Name" + id,
                    Guid = Guid.NewGuid()
                };

                return row;
            }

            /// <summary>Creates a constant.</summary>
            /// <param name="id">The identifier.</param>
            /// <returns>The new constant.</returns>
            public static ModelWithFieldsOfDifferentTypes CreateConstant(int id)
            {
                var row = new ModelWithFieldsOfDifferentTypes
                {
                    Id = id,
                    Bool = id % 2 == 0,
                    DateTime = new DateTime(1979, id % 12 + 1, id % 28 + 1),
                    Double = 1.11d + id,
                    LongId = 999 + id,
                    Name = "Name" + id,
                    Guid = Guid.Empty
                };

                return row;
            }

            /// <summary>
            ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
            ///     <see cref="T:System.Object" />.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns>
            ///     true if the specified <see cref="T:System.Object" /> is equal to the current
            ///     <see cref="T:System.Object" />; otherwise, false.
            /// </returns>
            public override bool Equals(object obj)
            {
                var other = obj as ModelWithFieldsOfDifferentTypes;
                if (other == null)
                {
                    return false;
                }

                try
                {
                    AssertIsEqual(this, other);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>Serves as a hash function for a particular type.</summary>
            /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
            public override int GetHashCode()
            {
                return (Id + Name + LongId + Double + DateTime).GetHashCode();
            }

            /// <summary>Assert is equal.</summary>
            /// <param name="actual">  The actual.</param>
            /// <param name="expected">The expected.</param>
            public static void AssertIsEqual(ModelWithFieldsOfDifferentTypes actual,
                ModelWithFieldsOfDifferentTypes expected)
            {
                Assert.That(actual.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.Name, Is.EqualTo(expected.Name));
                Assert.That(actual.LongId, Is.EqualTo(expected.LongId));
                Assert.That(actual.Bool, Is.EqualTo(expected.Bool));
                try
                {
                    Assert.That(actual.DateTime, Is.EqualTo(expected.DateTime));
                }
                catch (Exception)
                {
                    Assert.That(actual.DateTime, Is.EqualTo(expected.DateTime));
                }

                try
                {
                    Assert.That(actual.Double, Is.EqualTo(expected.Double));
                }
                catch (Exception)
                {
                    Assert.That(Math.Round(actual.Double, 10), Is.EqualTo(Math.Round(actual.Double, 10)));
                }
            }
        }

        /// <summary>Creates model with fields of different types.</summary>
        /// <returns>The new model with fields of different types.</returns>
        private void CreateModelWithFieldsOfDifferentTypes(int count = 10)
        {
            using (var conn = OpenDbConnection())
            {
                conn.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

                for (var i = 0; i < count; i++)
                {
                    conn.Insert(ModelWithFieldsOfDifferentTypes.Create(i));
                }
            }
        }

        /// <summary>Can update model with fields of different types table.</summary>
        [Test]
        public void Can_update_allColumns_ModelWithFieldsOfDifferentTypes_table_with_Implicit_Filter()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var row = conn.Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).FirstOrDefault();
                row.Name = "UpdatedName";

                conn.Update(row);

                var dbRow = conn.Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).FirstOrDefault();

                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);

                Assert.AreNotEqual(row.Name,
                    OpenDbConnection().First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
            }
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var two = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2);

                conn.UpdateAll<ModelWithFieldsOfDifferentTypes>(new {Name = "UpdatedName"});

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
                Assert.AreEqual(two.DateTime, conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).DateTime);
            }
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_and_type()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                conn.UpdateAll(new ModelWithFieldsOfDifferentTypes
                    {Name = "UpdatedName", DateTime = new DateTime(2000, 1, 1)});

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual(new DateTime(2000, 1, 1),
                    conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).DateTime);
                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
                Assert.AreEqual(new DateTime(2000, 1, 1),
                    conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).DateTime);
            }
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll<ModelWithFieldsOfDifferentTypes>(new {Name = "UpdatedName"}, x => x.Id == 1);

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual("Name1", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
                Assert.AreEqual(one.DateTime, conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).DateTime);
            }
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where_and_type()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll(
                    new ModelWithFieldsOfDifferentTypes {Name = "UpdatedName", DateTime = new DateTime(2000, 1, 1)},
                    x => x.Id == 1);

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual(Guid.Empty, conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Guid);
                Assert.AreEqual(new DateTime(2000, 1, 1),
                    conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).DateTime);
                Assert.AreEqual("Name1", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
            }
        }

        [Test]
        public void Can_update_multipleColumn_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll<ModelWithFieldsOfDifferentTypes>(
                    new
                    {
                        Name = "UpdatedName",
                        DateTime = new DateTime(2000, 1, 1),
                        Guid = Guid.Empty
                    },
                    x => x.Id == 1,
                    x => new {x.Name, x.Guid});

                var dbone = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);
                Assert.AreEqual("UpdatedName", dbone.Name);
                Assert.AreEqual(Guid.Empty, dbone.Guid);
                Assert.AreEqual(one.DateTime, dbone.DateTime);
            }
        }

        [Test]
        public void
            Can_update_multipleColumn_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where_and_type()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll(
                    new ModelWithFieldsOfDifferentTypes
                    {
                        Name = "UpdatedName",
                        DateTime = new DateTime(2000, 1, 1),
                        Guid = Guid.Empty
                    },
                    x => x.Id == 1,
                    x => new {x.Name, x.Guid});

                var dbone = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);
                Assert.AreEqual("UpdatedName", dbone.Name);
                Assert.AreEqual(Guid.Empty, dbone.Guid);
                Assert.AreEqual(one.DateTime, dbone.DateTime);
            }
        }

        [Test]
        public void Can_update_multipleColumns_ModelWithFieldsOfDifferentTypes_table_with_Implicit_Filter()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var row = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);
                row.Name = "UpdatedName";
                row.DateTime = new DateTime(2000, 1, 1);
                row.Guid = Guid.NewGuid();

                conn.Update(row, x => new {x.Name, x.DateTime});

                var dbRow = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                Assert.AreEqual(row.Name, dbRow.Name);
                Assert.AreEqual(row.DateTime, dbRow.DateTime);
                Assert.AreNotEqual(row.Guid, dbRow.Guid);

                Assert.AreNotEqual(row.Name,
                    OpenDbConnection().First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
            }
        }

        [Test]
        public void Can_update_singleColumn_ModelWithFieldsOfDifferentTypes_table_with_Implicit_Filter()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var row = conn.Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).FirstOrDefault();
                row.Name = "UpdatedName";
                row.DateTime = new DateTime(2000, 1, 1);

                conn.Update(row, x => x.Name);

                var dbRow = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                Assert.AreEqual(row.Name, dbRow.Name);
                Assert.AreNotEqual(row.DateTime, dbRow.DateTime);

                Assert.AreNotEqual(row.Name,
                    OpenDbConnection().First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
            }
        }

        [Test]
        public void Can_update_singleColumn_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll<ModelWithFieldsOfDifferentTypes>(
                    new {Name = "UpdatedName", DateTime = new DateTime(2000, 1, 1)}, x => x.Id == 1, x => x.Name);

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual("Name1", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
                Assert.AreEqual(one.DateTime, conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).DateTime);
            }
        }

        [Test]
        public void
            Can_update_singleColumn_ModelWithFieldsOfDifferentTypes_table_without_Implicit_Filter_with_where_and_type()
        {
            CreateModelWithFieldsOfDifferentTypes();
            using (var conn = OpenDbConnection())
            {
                var one = conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1);

                conn.UpdateAll(
                    new ModelWithFieldsOfDifferentTypes {Name = "UpdatedName", DateTime = new DateTime(2000, 1, 1)},
                    x => x.Id == 1, x => x.Name);

                Assert.AreEqual("UpdatedName", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).Name);
                Assert.AreEqual("Name1", conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 2).Name);
                Assert.AreEqual(one.DateTime, conn.First<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).DateTime);
            }
        }

        [Test]
        public void Tests_updates()
        {
            using (var conn = OpenDbConnection())
            {
                conn.Insert(new TestType2 {Id = 1, TextCol = "coucou", DateCol = DateTime.Now.AddDays(3)});

                var t = conn.FirstOrDefault<TestType2>(x => x.Id == 1);

                t.TextCol = "coucou updated";

                // Update TestType2 set TextCol = t.TextCol, DateCol = t.DateCol, dfsdd = .... WHERE Id = t.Id
                conn.Update(t);

                // Update TestType2 set TextCol = t.TextCol WHERE Id = t.Id
                conn.Update(t, x => x.TextCol);

                // Update TestType2 set TextCol = t.TextCol, DateCol = t.DateCol WHERE Id = t.Id
                conn.Update(t, x => new {x.TextCol, x.DateCol});

                // Update TestType2 set TextCol = "coucou massive update"
                conn.UpdateAll(new TestType2 {TextCol = "coucou massive update"}, null, x => x.TextCol);

                // Update TestType2 set TextCol = "coucou massive update" where Id = 1 
                conn.UpdateAll(new TestType2 {TextCol = "coucou massive update", DateCol = DateTime.Now},
                    x => x.Id == 1, x => new {x.TextCol, x.DateCol});

                // Update TestType2 set TextCol = "coucou massive update" where Id = 1 
                conn.UpdateAll<TestType2>(new {TextCol = "coucou massive update from anonymous"}, x => x.Id == 1);

                // Update TestType2 set DateCol = DateTime.Now where Id = 1 
                conn.UpdateAll<TestType2>(
                    new {TextCol = "coucou massive update from anonymous", DateCol = DateTime.Now}, x => x.Id == 1,
                    x => x.DateCol);
            }
        }
    }
}