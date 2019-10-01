using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Dynamic
{
    public class DynamicSelectStatement : DynamicCountStatement
    {
        private readonly IDialectProvider _dialectProvider;

        public DynamicSelectStatement(IDialectProvider dialectProvider)
            :base(dialectProvider)
        {
            _dialectProvider = dialectProvider;
        }

        public DynamicSelectStatement Select(params string[] columns)
        {
            Statement.Columns.AddRange(columns.Select(x => _dialectProvider.GetQuotedColumnName(x)));
            return this;
        }

        public DynamicSelectStatement SelectRaw(params string[] columns)
        {
            Statement.Columns.AddRange(columns);
            return this;
        }

        public DynamicSelectStatement Select<T>(string columnName, Expression<Func<T, T>> keySelector)
        {
            Statement.Columns.Add(GetExpressionVisitor<T>(columnName).VisitExpression(keySelector));
            return this;
        }

        public new DynamicSelectStatement From(string tableName)
        {
            base.From(tableName);
            return this;
        }

        public new DynamicSelectStatement Distinct(bool distinct = true)
        {
            base.Distinct(distinct);
            return this;
        }

        public new DynamicSelectStatement GroupBy(params string[] columns)
        {
            base.GroupBy(columns);
            return this;
        }

        public new DynamicSelectStatement GroupBy<T>(string columnName, Expression<Func<T, T>> keySelector)
        {
            base.GroupBy(columnName, keySelector);
            return this;
        }

        public new DynamicSelectStatement Where<T>(string columnName, Expression<Func<T, bool>> func)
        {
            base.And(columnName, func);
            return this;
        }
        
        public new DynamicSelectStatement And<T>(string columnName, Expression<Func<T, bool>> func)
        {
            base.Where(GetExpressionVisitor<T>(columnName), func);
            return this;
        }

        public new DynamicSelectStatement Or<T>(string columnName, Expression<Func<T, bool>> func)
        {
            base.Where(GetExpressionVisitor<T>(columnName), func, "OR");
            return this;
        }
        
        public new DynamicSelectStatement AndRaw(string condition)
        {
            return (DynamicSelectStatement)base.Where(condition, "AND");
        }
        public new DynamicSelectStatement OrRaw(string condition)
        {
            return (DynamicSelectStatement)base.Where(condition, "OR");
        }

        public new DynamicSelectStatement Having<T>(string columnName, Expression<Func<T, bool>> predicate)
        {
            base.Having(columnName, predicate);
            return this;
        }

        public DynamicSelectStatement Limit(int offset, int maxRows)
        {
            Statement.Offset = offset;
            Statement.MaxRows = maxRows;
            return this;
        }

        public DynamicSelectStatement OrderBy(string columnName)
        {
            Statement.OrderByExpression.Clear();
            Statement.OrderByExpression.Append(columnName);
            Statement.OrderByExpression.Append(" ASC");
            return this;
        }

        public DynamicSelectStatement ThenBy(string columnName)
        {
            Statement.OrderByExpression.Append(",");
            Statement.OrderByExpression.Append(columnName);
            Statement.OrderByExpression.Append(" ASC");
            return this;
        }

        public DynamicSelectStatement OrderByDescending(string columnName)
        {
            Statement.OrderByExpression.Clear();
            Statement.OrderByExpression.Append(columnName);
            Statement.OrderByExpression.Append(" DESC");
            return this;
        }

        public DynamicSelectStatement ThenByDescending(string columnName)
        {
            Statement.OrderByExpression.Append(",");
            Statement.OrderByExpression.Append(columnName);
            Statement.OrderByExpression.Append(" DESC");
            return this;
        }
    }
}