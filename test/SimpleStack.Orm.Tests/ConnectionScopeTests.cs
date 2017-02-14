using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{

	public partial class ExpressionTests 
	{
		[Test]
		public void Test_ConnectionScope_Throw_Exception()
		{
			var connFac = new OrmConnectionFactory(_dialectProvider, ConnectionString);

			Assert.Throws(typeof(Exception), () =>
			{
				using (var scope1 = new TransactionScope(connFac))
				{
					using (var scope2 = new TransactionScope(connFac))
					{
						using (var scope3 = new TransactionScope(connFac))
						{
							throw new Exception();
							scope3.Complete();
						}
						scope2.Complete();
					}
					scope1.Complete();
				}
			});
		}


		[Test]
		public void ConnectionScope_1()
		{
			var connFac = new OrmConnectionFactory(_dialectProvider, ConnectionString);

			using (var scope = new TransactionScope(connFac))
			{
				scope.Connection.Select<TestType>();

				scope.Complete();

				Assert.IsTrue(scope.Completed);
			}
		}
	}

}
