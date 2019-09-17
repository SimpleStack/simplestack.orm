using System;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Typed
{
    public abstract class TypedWhereStatement<T>
    {
        private readonly IDialectProvider _dialectProvider;
        private readonly ModelDefinition _modelDefinition;


        internal TypedWhereStatement(IDialectProvider dialectProvider)
        {
            _dialectProvider = dialectProvider;
            _modelDefinition = ModelDefinition<T>.Definition;

            WhereStatement.TableName = _dialectProvider.GetQuotedTableName(
                _modelDefinition.Alias ?? _modelDefinition.ModelName);
        }

        private WhereStatement WhereStatement => GetWhereStatement();

        protected abstract WhereStatement GetWhereStatement();

        private TableWhereExpresionVisitor<T> GetExpressionVisitor()
        {
            return new TableWhereExpresionVisitor<T>(_dialectProvider, WhereStatement.Parameters, _modelDefinition);
        }
        
        /// <summary>
        /// Used to overwrite the tablename generated from the Typed parameter
        /// </summary>
        /// <param name="tableName">The new table name</param>
        /// <returns></returns>
        public virtual TypedWhereStatement<T> From(string tableName)
        {
            WhereStatement.TableName = _dialectProvider.GetQuotedTableName(tableName);
            return this;
        }
        
        public virtual TypedWhereStatement<T> Clear()
        {
            WhereStatement.Clear();
            return this;
        }

        /// <summary>Wheres the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public TypedWhereStatement<T> Where(Expression<Func<T, bool>> predicate)
        {
            And(predicate);

            return this;
        }

        /// <summary>Ands the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public TypedWhereStatement<T> And(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                return this;

            Where(GetExpressionVisitor(), predicate);
            return this;
        }

        /// <summary>Ors the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public TypedWhereStatement<T> Or(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                return this;

            Where(GetExpressionVisitor(), predicate, "OR");
            return this;
        }

        internal void Where(ExpressionVisitor visitor, Expression<Func<T, bool>> func, string op = "AND")
        {
            var where = visitor.VisitExpression(func);

            if (WhereStatement.WhereExpression.Length > 0)
            {
                WhereStatement.WhereExpression.Insert(0, " (");
                WhereStatement.WhereExpression.Append(" ");
                WhereStatement.WhereExpression.Append(op);
                WhereStatement.WhereExpression.Append(" ");
                WhereStatement.WhereExpression.Append(where);
                WhereStatement.WhereExpression.Append(')');
            }
            else
            {
                WhereStatement.WhereExpression.Append(where);
            }
        }
    }
}