﻿using System;
using System.Data;
using System.Linq;
using Dapper;
using SimpleStack.Orm.Attributes;
using NUnit.Framework;
using SimpleStack.Orm.Expressions;
using SimpleStack.Orm.Expressions.Statements.Typed;

namespace SimpleStack.Orm.Tests
{
	[TestFixture]
	public partial class ExpressionTests
	{
		[Test]
		public void Can_select_count_without_parameters()
		{
			CreateModelWithFieldsOfDifferentTypes(10);
			using (var conn = OpenDbConnection())
			{
				Assert.AreEqual(10, conn.Count<ModelWithFieldsOfDifferentTypes>());
			}
		}
		[Test]
		public void Can_select_count_with_where_Statement()
		{
			CreateModelWithFieldsOfDifferentTypes(10);
			using (var conn = OpenDbConnection())
			{
				var v = new TypedSelectStatement<ModelWithFieldsOfDifferentTypes>(_dialectProvider);
				v.Where(x => x.Bool);

				var select = _dialectProvider.ToSelectStatement(v.Statement,CommandFlags.None);
				
				Assert.AreEqual(5, conn.Count<ModelWithFieldsOfDifferentTypes>(x =>
				{
					x.Where(q => q.Bool);
				}));
			}
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

			using (var conn = OpenDbConnection())
			{
				Assert.AreEqual(4, conn.Count<Person>(x =>
				{
					x.Distinct();
					x.Select(y => y.Age);
				})); // SELECT COUNT (DISTINCT Age) FROM Person
			}
		}
	}
}