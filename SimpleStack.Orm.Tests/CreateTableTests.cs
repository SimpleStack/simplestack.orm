using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.Tests
{
	public partial class ExpressionTests
	{
		[Test]
		public void CreateTableIfNotExistDidNotDropTable()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				Assert.AreEqual(3,db.Count<Member>());

				db.CreateTableIfNotExists<Member>();

				Assert.AreEqual(3, db.Count<Member>());
			}
		}

		[Test]
		public void CreateTableWithoutDropThrowExceptionIfTableExists()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				Assert.Throws<Exception>(() =>{db.CreateTable<Member>(false);});
				
				Assert.AreEqual(3, db.Count<Member>());
			}
		}
	}
}
