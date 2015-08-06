using System;
using System.Text;
using SimpleStack.Orm;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.SqlServer
{
	/// <summary>A SQL server expression visitor.</summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public class SqlServerExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
		public SqlServerExpressionVisitor(IDialectProvider dialectProvider) : base(dialectProvider)
		{
		}
	}
}