using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        private void SetupContext()
        {
            using (var c = OpenDbConnection())
            {
                c.CreateTable<TestType2>(true);
                c.Insert(new TestType2
                {
                    Id = 1, BoolCol = true, DateCol = new DateTime(2012, 11, 2, 3, 4, 5), TextCol = "asdf",
                    EnumCol = TestEnum.Val0, GuidCol = Guid.Empty
                });
                c.Insert(new TestType2
                {
                    Id = 2, BoolCol = true, DateCol = new DateTime(2012, 2, 1), TextCol = "asdf123",
                    EnumCol = TestEnum.Val1, GuidCol = Guid.NewGuid(),
                    NullableDateCol = new DateTime(2014,1,25)
                });
                c.Insert(new TestType2
                {
                    Id = 3, BoolCol = true, DateCol = new DateTime(2012, 3, 1), TextCol = "qwer",
                    EnumCol = TestEnum.Val2, GuidCol = Guid.NewGuid()
                });
                c.Insert(new TestType2
                {
                    Id = 4, BoolCol = false, DateCol = new DateTime(2012, 4, 1), TextCol = "qwer123",
                    EnumCol = TestEnum.Val3, GuidCol = Guid.NewGuid(),
                    NullableDateCol = new DateTime(2014,1,25)
                });
            }
        }

        private IEnumerable<string> GetStrings()
        {
            yield return "asdf";
            yield return "qwer";
        }

        /// <summary>Method returning int.</summary>
        /// <param name="val">The value.</param>
        /// <returns>An int.</returns>
        private int MethodReturningInt(int val)
        {
            return val;
        }

        /// <summary>Method returning int.</summary>
        /// <returns>An int.</returns>
        private int MethodReturningInt()
        {
            return 1;
        }

        /// <summary>Method returning enum.</summary>
        /// <returns>A TestEnum.</returns>
        private TestEnum MethodReturningEnum()
        {
            return TestEnum.Val0;
        }

        /// <summary>Can select by constant enum.</summary>
        [Test]
        public void Can_Select_by_const_enum()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.EnumCol == TestEnum.Val0);
                Assert.AreEqual(1, target.Count());
                target = conn.Select<TestType2>(q => TestEnum.Val0 == q.EnumCol);
                Assert.AreEqual(1, target.Count());
            }
        }

        /// <summary>Can select by constant int.</summary>
        [Test]
        public void Can_Select_by_const_guid()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.GuidCol == Guid.Empty).ToArray();

                Assert.AreEqual(1, target.Length);
                Assert.AreEqual(Guid.Empty, target[0].GuidCol);
            }
        }

        /// <summary>Can select by constant int.</summary>
        [Test]
        public void Can_Select_by_const_int()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var tt = conn.Select<TestType2>().ToArray();

                var target = conn.Select<TestType2>(q => q.Id == 1).ToArray();
                Assert.AreEqual(1, target.Length);
                Assert.AreEqual("asdf", target[0].TextCol);
            }
        }

        /// <summary>Can select by enum returned by method.</summary>
        [Test]
        public void Can_Select_by_enum_returned_by_method()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.EnumCol == MethodReturningEnum());
                Assert.AreEqual(1, target.Count());
            }
        }

        [Test]
        public void Can_Select_by_guid()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var rr = conn.First<TestType2>(x => x.Id == 1);
                var target = conn.Select<TestType2>(q => q.GuidCol == rr.GuidCol).ToArray();

                Assert.AreEqual(1, target.Length);
                Assert.AreEqual(rr.GuidCol, target[0].GuidCol);
            }
        }

        [Test]
        public void Can_Select_by_new_guid()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var rr = conn.First<TestType2>(x => x.Id == 1).GuidCol.ToString("N");
                var target = conn.Select<TestType2>(q => q.GuidCol == new Guid(rr)).ToArray();

                Assert.AreEqual(1, target.Length);
                Assert.AreEqual(rr, target[0].GuidCol.ToString("N"));
            }
        }

        /// <summary>Can select by value returned by method with parameter.</summary>
        [Test]
        public void Can_Select_by_value_returned_by_method_with_param()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.Id == MethodReturningInt(1));
                Assert.AreEqual(1, target.Count());
            }
        }

        /// <summary>Can select by value returned by method without parameters.</summary>
        [Test]
        public void Can_Select_by_value_returned_by_method_without_params()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.Id == MethodReturningInt());
                Assert.AreEqual(1, target.Count());
            }
        }

        [Test]
        public void Can_Select_dynamic_using_ToUpper_ToLower_Substring_on_string_property_of_T()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select("TestType2",
                    q => q.Where("textcol", (string x) => x.ToLower().Substring(0, 2) == "as"));
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void Can_Select_Scalar_using_Date_Functions_And_Properties()
        {
            SetupContext();

            Assert.AreEqual(1, new DateTime(2010, 1, 1).Quarter());
            Assert.AreEqual(1, new DateTime(2010, 2, 1).Quarter());
            Assert.AreEqual(1, new DateTime(2010, 3, 1).Quarter());
            Assert.AreEqual(2, new DateTime(2010, 4, 1).Quarter());
            Assert.AreEqual(2, new DateTime(2010, 5, 1).Quarter());
            Assert.AreEqual(2, new DateTime(2010, 6, 1).Quarter());
            Assert.AreEqual(3, new DateTime(2010, 7, 1).Quarter());
            Assert.AreEqual(3, new DateTime(2010, 8, 1).Quarter());
            Assert.AreEqual(3, new DateTime(2010, 9, 1).Quarter());
            Assert.AreEqual(4, new DateTime(2010, 10, 1).Quarter());
            Assert.AreEqual(4, new DateTime(2010, 11, 1).Quarter());
            Assert.AreEqual(4, new DateTime(2010, 12, 1).Quarter());

            using (var conn = OpenDbConnection())
            {
                Assert.AreEqual(2012, conn.GetScalar<TestType2, int>(x => x.DateCol.Year));
                Assert.AreEqual(11, conn.GetScalar<TestType2, int>(x => x.DateCol.Month));
                Assert.AreEqual(2, conn.GetScalar<TestType2, int>(x => x.DateCol.Day));
                Assert.AreEqual(3, conn.GetScalar<TestType2, int>(x => x.DateCol.Hour));
                Assert.AreEqual(4, conn.GetScalar<TestType2, int>(x => x.DateCol.Minute));
                Assert.AreEqual(5, conn.GetScalar<TestType2, int>(x => x.DateCol.Second));

                Assert.AreEqual(3, conn.Select<TestType2>(x => x.DateCol.Hour == 3).First().DateCol.Hour);
                
                //Check nullable
                Assert.AreEqual(2014,conn.GetScalar<TestType2,int>(x => x.NullableDateCol.Value.Year, x => x.NullableDateCol.HasValue));
            }
        }

        [Test]
        public void Can_Select_Scalar_using_Date_In_Where_Functions()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                //SELECT "id", "textcol", "boolcol", "datecol", "enumcol", "complexobjcol"
                //FROM "testtype2"
                //WHERE (date_part('month', "datecol") = 11)

                var v = conn.FirstOrDefault<TestType2>(x => x.DateCol.Month == 11);

                Assert.AreEqual(2012, v.DateCol.Year);
                Assert.AreEqual(11, v.DateCol.Month);
            }
        }

        [Test]
        public void Can_Select_Scalar_using_Date_Property_Functions()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                Assert.AreEqual(2012, conn.GetScalar<TestType2, int>(x => x.DateCol.Year));
            }
        }

        [Test]
        public void Can_Select_Scalar_using_MAX()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var maxDate = conn.GetScalar<TestType2, DateTime>(x => Sql.Max(x.DateCol));
                Assert.AreEqual(new DateTime(2012, 11, 2, 3, 4, 5), maxDate);
            }
        }

        [Test]
        public void Can_Select_Scalar_using_MIN()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var minDate = conn.GetScalar<TestType2, DateTime>(x => Sql.Min(x.DateCol));
                Assert.AreEqual(new DateTime(2012, 2, 1), minDate);
            }
        }

        [Test]
        public void Can_Select_Scalar_using_SUM()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var sumIds = conn.GetScalar<TestType2, int>(x => Sql.Sum(x.Id));
                Assert.AreEqual(10, sumIds);
            }
        }

        /// <summary>Can select using constant bool value.</summary>
        [Test]
        public void Can_Select_using_Constant_Bool_Value()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.BoolCol);
                Assert.AreEqual(3, target.Count());
                target = conn.Select<TestType2>(q => q.BoolCol);
                Assert.AreEqual(3, target.Count());
                target = conn.Select<TestType2>(q => !q.BoolCol);
                Assert.AreEqual(1, target.Count());
                target = conn.Select<TestType2>(q => q.BoolCol == false);
                Assert.AreEqual(1, target.Count());
            }
        }

        /// <summary>Can select using in.</summary>
        [Test]
        public void Can_Select_using_IN()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(x => new[] {"asdf", "qwer"}.Contains(x.TextCol));
                Assert.AreEqual(2, target.Count());

                target = conn.Select<TestType2>(x => GetStrings().ToArray().Contains(x.TextCol));
                Assert.AreEqual(2, target.Count());
                target = conn.Select<TestType2>(x => GetStrings().Contains(x.TextCol));
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void Can_Select_using_IN_using_extenral_list()
        {
            SetupContext();

            var invalues = new List<int>();
            invalues.Add(1);
            invalues.Add(2);
            invalues.Add(3);

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => invalues.Contains(q.Id));
                Assert.AreEqual(3, target.Count());
            }
        }

        /// <summary>Can select using in using int array.</summary>
        [Test]
        public void Can_Select_using_IN_using_int_array()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => new[] {1, 2, 3}.Contains(q.Id));
                Assert.AreEqual(3, target.Count());
            }
        }

        /// <summary>Can select using in using object array.</summary>
        [Test]
        public void Can_Select_using_IN_using_object_array()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => new object[] {1, 2, 3}.Contains(q.Id));
                Assert.AreEqual(3, target.Count());
            }
        }

        /// <summary>Can select using startswith.</summary>
        [Test]
        public void Can_Select_using_Startswith()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.TextCol.StartsWith("asdf"));
                Assert.AreEqual(2, target.Count());
            }
        }

        /// <summary>Can select using to lower on string property of field.</summary>
        [Test]
        public void Can_Select_using_ToLower_on_string_property_of_field()
        {
            SetupContext();
            var obj = new TestType2 {TextCol = "ASDF"};

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.TextCol == obj.TextCol.ToLower());
                Assert.AreEqual(1, target.Count());
            }
        }

        /// <summary>Can select using to upper on string property of t.</summary>
        [Test]
        public void Can_Select_using_ToUpper_on_string_property_of_T()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.TextCol.ToUpper() == "ASDF");
                Assert.AreEqual(1, target.Count());
            }
        }

        [Test]
        public void Can_Select_using_ToUpper_ToLower_Substring_on_string_property_of_T()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.TextCol.ToUpper().ToLower().Substring(0, 2) == "as");
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void Can_Select_With_and_clear_limit()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q =>
                {
                    q.OrderBy(x => x.Id);
                    q.Limit(1, 2);
                    q.Limit();
                }).ToArray();
                Assert.AreEqual(4, target.Length);
            }
        }

        [Test]
        public void Can_Select_With_Rows()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q =>
                {
                    q.OrderBy(x => x.Id);
                    q.Limit(1);
                }).ToArray();
                Assert.AreEqual(1, target.Length);
                Assert.AreEqual(1, target[0].Id);
            }
        }

        [Test]
        public void Can_Select_With_Skip_And_Rows()
        {
            SetupContext();

            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q =>
                {
                    q.OrderBy(x => x.Id);
                    q.Limit(2, 2);
                }).ToArray();
                Assert.AreEqual(2, target.Length);
                Assert.AreEqual(3, target[0].Id);
                Assert.AreEqual(4, target[1].Id);
            }
        }

        /// <summary>Can selelct using chained string operations.</summary>
        [Test]
        public void Can_Selelct_using_chained_string_operations()
        {
            SetupContext();
            var value = "ASDF";
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.TextCol.ToUpper().StartsWith(value));
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void Can_Select_Using_Nullable_HasValue()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.NullableDateCol.HasValue);
                Assert.AreEqual(2, target.Count());
            }
        }
        
        
        [Test]
        public void Can_Select_Using_Null_comparison()
        {
            SetupContext();
            using (var conn = OpenDbConnection())
            {
                var target = conn.Select<TestType2>(q => q.NullableDateCol == null);
                Assert.AreEqual(2, target.Count());
            }
        }
    }

    /// <summary>Values that represent TestEnum.</summary>
    public enum TestEnum
    {
        [Alias("ZERO")]
        /// <summary>An enum constant representing the value 0 option.</summary>
        Val0 = 0,

        [Alias("ONE")]
        /// <summary>An enum constant representing the value 1 option.</summary>
        Val1,

        [Alias("TWO")]
        /// <summary>An enum constant representing the value 2 option.</summary>
        Val2,

        [Alias("THREE")]
        /// <summary>An enum constant representing the value 3 option.</summary>
        Val3
    }

    /// <summary>A test type.</summary>
    public class TestType2
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>Gets or sets the text col.</summary>
        /// <value>The text col.</value>
        public string TextCol { get; set; }

        /// <summary>Gets or sets a value indicating whether the col.</summary>
        /// <value>true if col, false if not.</value>
        public bool BoolCol { get; set; }

        /// <summary>Gets or sets the Date/Time of the date col.</summary>
        /// <value>The date col.</value>
        public DateTime DateCol { get; set; }
        
        /// <summary>Gets or sets the Date/Time of the date col.</summary>
        /// <value>The date col.</value>
        public DateTime? NullableDateCol { get; set; }

        /// <summary>Gets or sets the enum col.</summary>
        /// <value>The enum col.</value>
        public TestEnum EnumCol { get; set; }

        public Guid GuidCol { get; set; }

        /// <summary>Gets or sets the complex object col.</summary>
        /// <value>The complex object col.</value>
        public TestType2 ComplexObjCol { get; set; }

        public long LongCol { get; set; }
    }
}