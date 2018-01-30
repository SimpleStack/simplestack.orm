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
	    public SqliteExpressionVisitor(IDialectProvider dialectProvider) : base(dialectProvider)
	    {
	    }

        protected override string BindOperant(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.ExclusiveOr:
                    throw new NotSupportedException("SQLite does not support XOR");
            }

            return base.BindOperant(e);
        }
    }
}
