using System.Collections.Generic;
using System.Linq.Expressions;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.MySQL
{
	/// <summary>Description of MySqlExpressionVisitor.</summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public class MySqlExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
		/// <summary>Visit column access method.</summary>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected override object VisitColumnAccessMethod(MethodCallExpression m)
		{
			if (m.Method.Name == "StartsWith")
			{
				List<object> args = new List<object>();
				if (m.Arguments.Count == 1 && m.Arguments[0].NodeType == ExpressionType.Constant)
				{
					args.Add(((ConstantExpression)m.Arguments[0]).Value);
				}
				else
				{
					args.AddRange(VisitExpressionList(m.Arguments));
				}
				var quotedColName = Visit(m.Object);
				return
					new PartialSqlString(string.Format("LEFT( {0},{1})= {2} ", quotedColName, args[0].ToString().Length,
						AddParameter(args[0])));
			}

			return base.VisitColumnAccessMethod(m);
		}
	}
}