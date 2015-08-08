using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using NUnit.Framework;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
	public abstract partial class ExpressionTests
	{
		public class Member
		{
			[PrimaryKey]
			[AutoIncrement]
			public long Id { get; set; }
			[Index]
			[StringLength(255)]
			public string Val1 { get; set; }
			[StringLength(255)]
			public string Val2 { get; set; }
			[StringLength(255)]
			public string Val3 { get; set; }
			[StringLength(255)]
			public string Val4 { get; set; }
			[StringLength(255)]
			public string Val5 { get; set; }
			[StringLength(255)]
			public string Val6 { get; set; }
			[StringLength(255)]
			public string Val7 { get; set; }
			[StringLength(255)]
			public string Val8 { get; set; }
			[StringLength(255)]
			public string Val9 { get; set; }
			[StringLength(255)]
			public string Val10 { get; set; }
		}

		[Test]
		[NUnit.Framework.Ignore]
		public void BulkInsert()
		{
			var members = new List<Member>();
			for (int i = 0; i < 12500; i++)
			{
				members.Add(new Member()
				{
					Val1 = Guid.NewGuid().ToString("N"),
					Val2 = Guid.NewGuid().ToString("N"),
					Val3 = Guid.NewGuid().ToString("N"),
					Val4 = Guid.NewGuid().ToString("N"),
					Val5 = Guid.NewGuid().ToString("N"),
					Val6 = Guid.NewGuid().ToString("N"),
					Val7 = Guid.NewGuid().ToString("N"),
					Val8 = Guid.NewGuid().ToString("N"),
					Val9 = Guid.NewGuid().ToString("N"),
					Val10 = Guid.NewGuid().ToString("N")
				});
			}
			DateTime start;


			Console.WriteLine("Inserting 12500 Members using pure ADO.NET");
			using (var conn = OpenDbConnection())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				var cmd = conn.CreateCommand();
				foreach (var m in members)
				{
					cmd.CommandText = String.Format(
						"insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')", m.Val1,m.Val2,m.Val3,m.Val4,m.Val5,m.Val6,m.Val7,m.Val8,m.Val9,m.Val10);
					cmd.ExecuteNonQuery();
				}
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				Assert.AreEqual(12500, conn.Count<Member>());
			}

			Console.WriteLine("Inserting 12500 Members using pure ADO.NET and Transaction");
			using (var conn = OpenDbConnection())
			using (var trans = conn.BeginTransaction())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				var cmd = conn.CreateCommand();
				//cmd.Transaction = trans;
				foreach (var m in members)
				{
					cmd.CommandText = String.Format(
						"insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')", m.Val1, m.Val2, m.Val3, m.Val4, m.Val5, m.Val6, m.Val7, m.Val8, m.Val9, m.Val10);
					cmd.ExecuteNonQuery();
				}
				trans.Commit();
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
			}


			Console.WriteLine("Inserting 12500 Members using 'conn.Execute(query,obj)'");
			using (var conn = OpenDbConnection())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				foreach (var m in members)
				{
					conn.Execute("insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9,@val10)",m);
				}
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				Assert.AreEqual(12500, conn.Count<Member>());
			}
			

			Console.WriteLine("Inserting 12500 Members using 'conn.Execute(query,obj) and Transaction'");
			using (var conn = OpenDbConnection())
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,TimeSpan.MaxValue))
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				foreach (var m in members)
				{
					conn.Execute("insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9,@val10)", m, commandTimeout: 2147483);
				}
				scope.Complete();
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				using (var s = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
				{
					Assert.AreEqual(12500, conn.Count<Member>());
				}
			}
			Console.WriteLine("Inserting 12500 Members using conn.Insert<Member>(member)");
			using (var conn = OpenDbConnection())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				foreach (var m in members)
				{
					conn.Insert<Member>(m);
				}
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				Assert.AreEqual(12500, conn.Count<Member>());
			}

			Console.WriteLine("Inserting 12500 Members using conn.Insert<Member>(member) and Transaction");
			using (var conn = OpenDbConnection())
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				foreach (var m in members)
				{
					conn.Insert<Member>(m);
				}
				scope.Complete();
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				using (var s = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
				{
					Assert.AreEqual(12500, conn.Count<Member>());
				}
			}

			Console.WriteLine("Inserting 12500 Members using 'conn.Execute(query,IEnumerable)'");
			using (var conn = OpenDbConnection())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				conn.Execute("insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9,@val10)", members);
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				Assert.AreEqual(12500, conn.Count<Member>());
			}

			Console.WriteLine("Inserting 12500 Members using 'conn.Execute(query,IEnumerable) and Transaction'");
			using (var conn = OpenDbConnection())
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				conn.Execute("insert into Member(VAL1,VAL2,VAL3,VAL4,VAL5,VAL6,VAL7,VAL8,VAL9,VAL10) values(@val1,@val2,@val3,@val4,@val5,@val6,@val7,@val8,@val9,@val10)", members);
				scope.Complete();
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				using (var s = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
				{
					Assert.AreEqual(12500, conn.Count<Member>());
				}
			}
			
			Console.WriteLine("Inserting 12500 Members using conn.Insert<Member>(members)");
			using (var conn = OpenDbConnection())
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				conn.Insert<Member>(members);
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				Assert.AreEqual(12500, conn.Count<Member>());
			}
			Console.WriteLine("Inserting 12500 Members using conn.Insert<Member>(members) and Transaction");
			using (var conn = OpenDbConnection())
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
			{
				conn.CreateTable<Member>(true);
				start = DateTime.Now;
				conn.Insert<Member>(members);
				scope.Complete();
				Console.WriteLine("Duration : {0}", DateTime.Now.Subtract(start));
				using (var s = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.MaxValue))
				{
					Assert.AreEqual(12500, conn.Count<Member>());
				}
			}
		}
	}
}
