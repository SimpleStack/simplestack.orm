using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Dynamic
{
    public class DynamicSelectStatement : DynamicCountStatement
    {
        public DynamicSelectStatement(IDialectProvider dialectProvider)
            :base(dialectProvider)
        {
        }

        public DynamicSelectStatement Select(params string[] columns)
        {
            Statement.Columns.AddRange(columns);
            return this;
        }

        public DynamicSelectStatement Select<T>(string statement, Expression<Func<T, T>> keySelector)
        {
            Statement.Columns.Add(GetExpressionVisitor<T>(statement).VisitExpression(keySelector));
            return this;
        }

        public new DynamicSelectStatement From(string tableName, string schemaName = null)
        {
            base.From(tableName,schemaName);
            return this;
        }

        public new DynamicSelectStatement Distinct(bool distinct = true)
        {
            base.Distinct(distinct);
            return this;
        }

        public new DynamicSelectStatement GroupBy(params string[] statement)
        {
            base.GroupBy(statement);
            return this;
        }

        public new DynamicSelectStatement GroupBy<T>(string statement, Expression<Func<T, T>> keySelector)
        {
            base.GroupBy(statement, keySelector);
            return this;
        }

        public new DynamicSelectStatement Where<T>(string statement, Expression<Func<T, bool>> func)
        {
            base.And(statement, func);
            return this;
        }
        
        public new DynamicSelectStatement And<T>(string statement, Expression<Func<T, bool>> func)
        {
            base.Where(GetExpressionVisitor<T>(statement), func);
            return this;
        }

        public new DynamicSelectStatement Or<T>(string statement, Expression<Func<T, bool>> func)
        {
            base.Where(GetExpressionVisitor<T>(statement), func, "OR");
            return this;
        }
 
        public new DynamicSelectStatement Having<T>(string statement, Expression<Func<T, bool>> predicate)
        {
            base.Having(statement, predicate);
            return this;
        }

        public DynamicSelectStatement Limit(int offset, int maxRows)
        {
            Statement.Offset = offset;
            Statement.MaxRows = maxRows;
            return this;
        }

        public DynamicSelectStatement OrderBy(string statement)
        {
            Statement.OrderByExpression.Clear();
            Statement.OrderByExpression.Append(statement);
            Statement.OrderByExpression.Append(" ASC");
            return this;
        }

        public DynamicSelectStatement ThenBy(string statement)
        {
            Statement.OrderByExpression.Append(",");
            Statement.OrderByExpression.Append(statement);
            Statement.OrderByExpression.Append(" ASC");
            return this;
        }

        public DynamicSelectStatement OrderByDescending(string statement)
        {
            Statement.OrderByExpression.Clear();
            Statement.OrderByExpression.Append(statement);
            Statement.OrderByExpression.Append(" DESC");
            return this;
        }

        public DynamicSelectStatement ThenByDescending(string statement)
        {
            Statement.OrderByExpression.Append(",");
            Statement.OrderByExpression.Append(statement);
            Statement.OrderByExpression.Append(" DESC");
            return this;
        }
    }
}