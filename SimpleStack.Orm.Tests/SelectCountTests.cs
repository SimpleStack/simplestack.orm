using System;
using System.Data;
using System.Linq;
using SimpleStack.Orm.Attributes;
using NServiceKit.Text;
using NUnit.Framework;
using SimpleStack.Orm.MySQL;

namespace SimpleStack.Orm.Tests
{
	[TestFixture]
	public partial class ExpressionTests
	{
		[Test]
		public void Can_select_count_without_parameters()
		{
			CreateModelWithFieldsOfDifferentTypes(10);
			Assert.AreEqual(10, OpenDbConnection().Count<ModelWithFieldsOfDifferentTypes>());
		}
		[Test]
		public void Can_select_count_with_where_Statement()
		{
			CreateModelWithFieldsOfDifferentTypes(10);
			Assert.AreEqual(5, OpenDbConnection().Count<ModelWithFieldsOfDifferentTypes>(x => x.Bool));
		}

		[Test]
		public void Can_select_count_with_distinct()
		{
			OpenDbConnection().Insert(new Person { Id  = 1, Age = 5 });
			OpenDbConnection().Insert(new Person { Id = 2, Age = 10 });
			OpenDbConnection().Insert(new Person { Id = 3, Age = 10 });
			OpenDbConnection().Insert(new Person { Id = 4, Age = 20 });
			OpenDbConnection().Insert(new Person { Id = 5, Age = 20 });
			OpenDbConnection().Insert(new Person { Id = 6, Age = 25 });

			Assert.AreEqual(4, OpenDbConnection().Count<Person>(x =>
			{
				x.Distinct();
				x.Select(y => y.Age);
				return x;
			})); // SELECT COUNT (DISTINCT Age) FROM Person
		}
	}
}