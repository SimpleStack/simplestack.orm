using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
	public partial class ExpressionTests
	{
		[Test]
		public void Can_select_async()
		{
			var expected = new TestType()
			{
				IntColumn = 7,
				BoolColumn = true,
				StringColumn = "test"
			};

			EstablishContext(10, expected);
			using (var conn = OpenDbConnection())
			{
				var actual = conn.SelectAsync<TestType>(q => q.IntColumn == 7).Result;

				Assert.IsNotNull(actual);
				Assert.AreEqual(1, actual.Count());
				CollectionAssert.Contains(actual, expected);
			}
		}
	}
}
