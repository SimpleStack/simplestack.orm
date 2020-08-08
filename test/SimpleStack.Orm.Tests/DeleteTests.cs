using System;
using System.Linq;
using NUnit.Framework;

namespace SimpleStack.Orm.Tests
{
    public partial class ExpressionTests
    {
        public class NoPrimaryKey
        {
            public string Test { get; set; }
            public string Test2 { get; set; }
        }

        private static void AddMembers(OrmConnection db)
        {
            db.Insert(new Member {Val1 = "Salut"});
            db.Insert(new Member {Val1 = "Hello"});
            db.Insert(new Member {Val1 = "Hola"});
        }

        [Test]
        public void CanDeleteWithoutPrimaryKey()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<NoPrimaryKey>(true);

                var npk = new NoPrimaryKey {Test = "coucou", Test2 = "Hello"};

                db.Insert(npk);
                db.Insert(new NoPrimaryKey {Test = "Hola"});
                db.Insert(new NoPrimaryKey {Test = "Hi !"});
                db.Insert(new NoPrimaryKey {Test = "coucou"});
                db.Insert(new NoPrimaryKey {Test = "coucou", Test2 = "Hello"});

                Assert.AreEqual(5, db.Count<NoPrimaryKey>());

                //DELETE FROM NoPrimaryKey Where Test = 'coucou' AND test2 = 'Hello'
                db.Delete(npk);

                Assert.AreEqual(3, db.Count<NoPrimaryKey>());
            }
        }

        [Test]
        public void CanDeleteAllItems()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<Member>(true);

                AddMembers(db);

                Assert.AreEqual(3, db.Count<Member>());

                db.DeleteAll<Member>();

                Assert.AreEqual(0, db.Count<Member>());
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

        [Test]
        public void CanDeleteUsingWhere1()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<Member>(true);

                AddMembers(db);

                db.DeleteAll<Member>(x => x.Val1 == "Salut");

                var members = db.Select<Member>(visitor => visitor.OrderBy(x => x.Val1)).ToArray();

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

                db.DeleteAll<Member>(x => x.Val1.StartsWith("H"));

                var members = db.Select<Member>().ToArray();

                Assert.AreEqual(1, members.Length);
                Assert.AreEqual("Salut", members[0].Val1);
            }
        }
    }
}