using System;
using System.Data;
using System.Linq;
using SimpleStack.Orm.Attributes;
using NServiceKit.Text;
using NUnit.Framework;

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
					DateTime = new DateTime(1979, (id % 12) + 1, (id % 28) + 1),
					Double = 1.11d + id,
					LongId = 999 + id,
					Name = "Name" + id,
					Guid = Guid.Empty
				};

				return row;
			}

			/// <summary>
			/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />.
			/// </summary>
			/// <param name="obj">The object to compare with the current object.</param>
			/// <returns>
			/// true if the specified <see cref="T:System.Object" /> is equal to the current
			/// <see cref="T:System.Object" />; otherwise, false.
			/// </returns>
			public override bool Equals(object obj)
			{
				var other = obj as ModelWithFieldsOfDifferentTypes;
				if (other == null) return false;

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
				return (Id+Name+LongId+Double+DateTime).GetHashCode();
			}

			/// <summary>Assert is equal.</summary>
			/// <param name="actual">  The actual.</param>
			/// <param name="expected">The expected.</param>
			public static void AssertIsEqual(ModelWithFieldsOfDifferentTypes actual, ModelWithFieldsOfDifferentTypes expected)
			{
				Assert.That(actual.Id, Is.EqualTo(expected.Id));
				Assert.That(actual.Name, Is.EqualTo(expected.Name));
				Assert.That(actual.LongId, Is.EqualTo(expected.LongId));
				Assert.That(actual.Bool, Is.EqualTo(expected.Bool));
				try
				{
					Assert.That(actual.DateTime, Is.EqualTo(expected.DateTime));
				}
				catch (Exception ex)
				{
					Assert.That(actual.DateTime.RoundToSecond(), Is.EqualTo(expected.DateTime.RoundToSecond()));
				}
				try
				{
					Assert.That(actual.Double, Is.EqualTo(expected.Double));
				}
				catch (Exception ex)
				{
					Assert.That(Math.Round(actual.Double, 10), Is.EqualTo(Math.Round(actual.Double, 10)));
				}
			}
		}

		/// <summary>Creates model with fields of different types.</summary>
		/// <returns>The new model with fields of different types.</returns>
		private ModelWithFieldsOfDifferentTypes CreateModelWithFieldsOfDifferentTypes()
		{
			OpenDbConnection().CreateTable<ModelWithFieldsOfDifferentTypes>(true);

			var row = ModelWithFieldsOfDifferentTypes.Create(1);
			return row;
		}

		/// <summary>Can update model with fields of different types table.</summary>
		[Test]
		public void Can_update_ModelWithFieldsOfDifferentTypes_table()
		{
			var row = CreateModelWithFieldsOfDifferentTypes();

			OpenDbConnection().Insert(row);

			row.Name = "UpdatedName";

			OpenDbConnection().Update<ModelWithFieldsOfDifferentTypes>(row);

			var dbRow = OpenDbConnection().Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).FirstOrDefault();

			ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
		}

		/// <summary>Can update model with fields of different types table with filter.</summary>
		[Test]
		public void Can_update_ModelWithFieldsOfDifferentTypes_table_with_filter()
		{
			var row = CreateModelWithFieldsOfDifferentTypes();

			OpenDbConnection().Insert(row);

			row.Name = "UpdatedName";

			OpenDbConnection().Update(row, x => x.LongId <= row.LongId);

			var dbRow = OpenDbConnection().Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == 1).FirstOrDefault();

			ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
		}

		/// <summary>Can update with anonymous type and expression filter.</summary>
		[Test]
		public void Can_update_with_anonymousType_and_expr_filter()
		{
			var row = CreateModelWithFieldsOfDifferentTypes();

			OpenDbConnection().Insert(row);
			row.DateTime = DateTime.Now;
			row.Name = "UpdatedName";

			OpenDbConnection().Update<ModelWithFieldsOfDifferentTypes>(new { row.Name, row.DateTime },x => x.LongId >= row.LongId && x.LongId <= row.LongId);

			var dbRow = OpenDbConnection().Select<ModelWithFieldsOfDifferentTypes>(x => x.Id == row.Id).FirstOrDefault();
			Console.WriteLine(dbRow.Dump());
			ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
		}

		/// <summary>Can update with optional string parameters.</summary>
		//[Test]
		//public void Can_update_with_optional_string_params()
		//{
		//	var row = CreateModelWithFieldsOfDifferentTypes();

		//	db.Insert(row);
		//	row.Name = "UpdatedName";

		//	db.Update<ModelWithFieldsOfDifferentTypes>(set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

		//	var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
		//	Console.WriteLine(dbRow.Dump());
		//	ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
		//}

		/// <summary>Can update with table name and optional string parameters.</summary>
		//[Test]
		//public void Can_update_with_tableName_and_optional_string_params()
		//{
		//	var row = CreateModelWithFieldsOfDifferentTypes();

		//	db.Insert(row);
		//	row.Name = "UpdatedName";

		//	db.Update(table: "ModelWithFieldsOfDifferentTypes",
		//		set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

		//	var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
		//	Console.WriteLine(dbRow.Dump());
		//	ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
		//}

	}
}