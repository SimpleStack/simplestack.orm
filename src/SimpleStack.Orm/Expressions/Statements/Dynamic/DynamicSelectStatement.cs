using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Dynamic
{
    public class DynamicSelectStatement
    {
        private readonly IDialectProvider _dialectProvider;

        public DynamicSelectStatement(IDialectProvider dialectProvider)
        {
            _dialectProvider = dialectProvider;
        }

        public SelectStatement Statement { get; } = new SelectStatement();

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

        public DynamicSelectStatement From(string tableName)
        {
            Statement.TableName = _dialectProvider.GetQuotedTableName(tableName);
            return this;
        }

        public DynamicSelectStatement Distinct(bool distinct = true)
        {
            Statement.IsDistinct = distinct;
            return this;
        }

        public DynamicSelectStatement GroupBy(params string[] columns)
        {
            Statement.GroupByExpression.Clear();
            Statement.GroupByExpression.Append(columns.Aggregate((x, y) => x + "," + y));
            return this;
        }

        public DynamicSelectStatement GroupBy<T>(string columnName, Expression<Func<T, T>> keySelector)
        {
            if (Statement.GroupByExpression.Length > 0) Statement.GroupByExpression.Append(",");
            Statement.GroupByExpression.Append(GetExpressionVisitor<T>(columnName).VisitExpression(keySelector));
            return this;
        }

        public DynamicSelectStatement Where<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return And(columnName, func);
        }

        public DynamicSelectStatement And<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return Where(GetExpressionVisitor<T>(columnName), func);
        }

        public DynamicSelectStatement Or<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return Where(GetExpressionVisitor<T>(columnName), func, "OR");
        }

        public DynamicSelectStatement Having<T>(string columnName, Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
                Statement.HavingExpression.Append(
                    new ColumnWhereExpresionVisitor<T>(_dialectProvider, Statement.Parameters, columnName)
                        .VisitExpression(predicate));
            else
                Statement.HavingExpression.Clear();

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

        private ColumnWhereExpresionVisitor<T> GetExpressionVisitor<T>(string columnName)
        {
            return new ColumnWhereExpresionVisitor<T>(_dialectProvider, Statement.Parameters, columnName);
        }

        private DynamicSelectStatement Where<T>(ExpressionVisitor visitor, Expression<Func<T, bool>> func,
            string op = "AND")
        {
            if (Statement.WhereExpression.Length > 0)
            {
                Statement.WhereExpression.Append(" ");
                Statement.WhereExpression.Append(op);
                Statement.WhereExpression.Append(" ");
            }

            Statement.WhereExpression.Append(visitor.VisitExpression(func));
            return this;
        }
    }
}