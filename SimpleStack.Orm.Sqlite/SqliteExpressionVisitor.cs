using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.Sqlite
{
    /// <summary>Description of SqliteExpressionVisitor.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public class SqliteExpressionVisitor<T> : SqlExpressionVisitor<T>
    {
        /// <summary>Visit column access method.</summary>
        /// <param name="m">The MethodCallExpression to process.</param>
        /// <returns>An object.</returns>
		//protected override object VisitColumnAccessMethod(MethodCallExpression m)
		//{
		//	List<Object> args = this.VisitExpressionList(m.Arguments);
		//	var quotedColName = Visit(m.Object);
		//	string statement;

		//	switch (m.Method.Name)
		//	{
		//		case "Substring":
		//			var startIndex = Int32.Parse(args[0].ToString()) + 1;
		//			if (args.Count == 2)
		//			{
		//				var length = Int32.Parse(args[1].ToString());
		//				statement = string.Format("substr({0}, {1}, {2})", quotedColName, startIndex, length);
		//			}
		//			else
		//				statement = string.Format("substr({0}, {1})", quotedColName, startIndex);
		//			break;
		//		default:
		//			return base.VisitColumnAccessMethod(m);
		//	}
		//	return new PartialSqlString(statement);
		//}

        /// <summary>Visit SQL method call.</summary>
        /// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
        /// <param name="m">The MethodCallExpression to process.</param>
        /// <returns>An object.</returns>
		//protected override object VisitSqlMethodCall(MethodCallExpression m)
		//{
		//	List<Object> args = this.VisitExpressionList(m.Arguments);
		//	object quotedColName = args[0];
		//	args.RemoveAt(0);

		//	var statement = "";

		//	switch (m.Method.Name)
		//	{
		//		case "In":
		//			var member = Expression.Convert(m.Arguments[1], typeof(object));
		//			var lambda = Expression.Lambda<Func<object>>(member);
		//			var getter = lambda.Compile();

		//			var inArgs = Sql.Flatten(getter() as IEnumerable);

		//			var sIn = new StringBuilder();
		//			foreach (var e in inArgs)
		//			{
		//				sIn.AppendFormat("{0}{1}",
		//							 sIn.Length > 0 ? "," : "",
		//							 Config.DialectProvider.GetQuotedValue(e, e.GetType()));
		//			}
		//			statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn);
		//			break;
		//		case "Desc":
		//			statement = string.Format("{0} DESC", quotedColName);
		//			break;
		//		case "As":
		//			statement = string.Format("{0} As {1}", quotedColName,
		//				Config.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
		//			break;
		//		case "Sum":
		//		case "Count":
		//		case "Min":
		//		case "Max":
		//		case "Avg":
		//			statement = string.Format("{0}({1}{2})",
		//								 m.Method.Name,
		//								 quotedColName,
		//								 args.Count == 1 ? string.Format(",{0}", args[0]) : "");
		//			break;
		//		default:
		//			throw new NotSupportedException();
		//	}

		//	return new PartialSqlString(statement);
		//}
	    public SqliteExpressionVisitor(IDialectProvider dialectProvider) : base(dialectProvider)
	    {
	    }
    }
}
