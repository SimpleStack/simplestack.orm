using System;
using System.Linq.Expressions;
using System.Text;
using SimpleStack.Orm;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.SqlServer
{
	/// <summary>A SQL server expression visitor.</summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public class SqlServerExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
	    protected override string BindOperant(ExpressionType e)
	    {
	        switch (e)
	        {
	            case ExpressionType.LeftShift:
	            case ExpressionType.RightShift:
	                throw new NotSupportedException("SQLServer does not support Bit shift << or >>");
            }

	        return base.BindOperant(e);
	    }

        public SqlServerExpressionVisitor(IDialectProvider dialectProvider) : base(dialectProvider)
		{
		}
	}
}