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
		public void CanDeleteAllItems()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				Assert.AreEqual(3,db.Count<Member>());

				db.DeleteAll<Member>();

				Assert.AreEqual(0, db.Count<Member>());
			}
		}

		[Test]
		public void CanDeleteUsingWhere1()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				db.Delete<Member>(x => x.Val1 == "Salut");

				var members = db.Select<Member>(visitor => { visitor.OrderBy(x => x.Val1);
					                                           return visitor;
				}).ToArray();

				Assert.AreEqual(2, members.Length);
				Assert.AreEqual("Hello", members[0].Val1);
				Assert.AreEqual("Hola", members[1].Val1);
			}
		}

		[Test]
		public void CanDeleteUsingWhere2()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				db.Delete<Member>(x => x.Val1.StartsWith("H"));

				var members = db.Select<Member>().ToArray();

				Assert.AreEqual(1, members.Length);
				Assert.AreEqual("Salut", members[0].Val1);
			}
		}

		[Test]
		public void CanDeleteUsingPrimaryKey()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Member>(true);

				AddMembers(db);

				var members = db.Select<Member>().ToArray();

				db.Delete(members[1]);

				members = db.Select<Member>().ToArray();

				Assert.AreEqual(2, members.Length);
			}
		}

		private static void AddMembers(OrmConnection db)
		{
			db.Insert(new Member { Val1 = "Salut" });
			db.Insert(new Member { Val1 = "Hello" });
			db.Insert(new Member { Val1 = "Hola" });
		}
	}
}
