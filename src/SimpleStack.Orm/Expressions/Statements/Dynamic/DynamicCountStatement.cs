using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Dynamic
{
    public class DynamicCountStatement
    {
        private readonly IDialectProvider _dialectProvider;

        public DynamicCountStatement(IDialectProvider dialectProvider)
        {
            _dialectProvider = dialectProvider;
        }

        public SelectStatement Statement { get; } = new SelectStatement();

        public DynamicCountStatement From(string tableName)
        {
            Statement.TableName = _dialectProvider.GetQuotedTableName(tableName);
            return this;
        }

        public DynamicCountStatement Distinct(bool distinct = true)
        {
            Statement.IsDistinct = distinct;
            return this;
        }

        public DynamicCountStatement GroupBy(params string[] columns)
        {
            Statement.GroupByExpression.Clear();
            Statement.GroupByExpression.Append(columns.Aggregate((x, y) => x + "," + y));
            return this;
        }

        public DynamicCountStatement GroupBy<T>(string columnName, Expression<Func<T, T>> keySelector)
        {
            if (Statement.GroupByExpression.Length > 0) Statement.GroupByExpression.Append(",");
            Statement.GroupByExpression.Append(GetExpressionVisitor<T>(columnName).VisitExpression(keySelector));
            return this;
        }

        public DynamicCountStatement Where<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return And(columnName, func);
        }

        public DynamicCountStatement And<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return Where(GetExpressionVisitor<T>(columnName), func);
        }

        public DynamicCountStatement Or<T>(string columnName, Expression<Func<T, bool>> func)
        {
            return Where(GetExpressionVisitor<T>(columnName), func, "OR");
        }

        public DynamicCountStatement Having<T>(string columnName, Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
                Statement.HavingExpression.Append(
                    new ColumnWhereExpresionVisitor<T>(_dialectProvider, Statement.Parameters, columnName)
                        .VisitExpression(predicate));
            else
                Statement.HavingExpression.Clear();

            return this;
        }

        internal ColumnWhereExpresionVisitor<T> GetExpressionVisitor<T>(string columnName)
        {
            return new ColumnWhereExpresionVisitor<T>(_dialectProvider, Statement.Parameters, columnName);
        }

        internal DynamicCountStatement Where<T>(ExpressionVisitor visitor, Expression<Func<T, bool>> func,
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