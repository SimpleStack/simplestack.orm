using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Typed
{
    public class TypedUpdateStatement<T> : TypedWhereStatement<T>
    {
        private readonly IDialectProvider _dialectProvider;
        private readonly ModelDefinition _modelDefinition = typeof(T).GetModelDefinition();

        public TypedUpdateStatement(IDialectProvider dialectProvider)
            : base(dialectProvider)
        {
            _dialectProvider = dialectProvider;
            Statement.TableName = _dialectProvider.GetQuotedTableName(_modelDefinition);
        }

        public UpdateStatement Statement { get; } = new UpdateStatement();

        /// <summary>
        ///     Override name of the table to insert values into.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TypedUpdateStatement<T> Table(string tableName, string schemaName = null)
        {
            Statement.TableName = _dialectProvider.GetQuotedTableName(tableName, schemaName);
            return this;
        }

        public TypedUpdateStatement<T> ValuesOnly(T values)
        {
            return Values(values, null, true);
        }

        public TypedUpdateStatement<T> ValuesOnly<TKey>(T values, Expression<Func<T, TKey>> onlyFields)
        {
            return Values(values, GetFieldsExpressionVisitor(true)
                .VisitExpression(onlyFields)
                .Split(','), true);
        }

        public TypedUpdateStatement<T> Values(T values)
        {
            return Values(values, null, false);
        }

        public TypedUpdateStatement<T> Values<TKey>(object values, Expression<Func<T, TKey>> onlyFields)
        {
            return onlyFields != null
                ? Values(values, GetFieldsExpressionVisitor(true)
                    .VisitExpression(onlyFields)
                    .Split(','), false)
                : Values(values, null, false);
        }

        private TypedUpdateStatement<T> Values(object values, IEnumerable<string> onlyFields,
            bool addPrimaryKeyWhereCondition)
        {
            //Filter computed fields
            var fields = values.GetType().GetModelDefinition().FieldDefinitions.Where(
                fieldDef => !fieldDef.IsComputed &&
                            !fieldDef.AutoIncrement &&
                            !fieldDef.IsPrimaryKey);

            //Filter fields not on the onlyFields list
            if (onlyFields != null)
            {
                var fieldsArray = onlyFields as string[] ?? onlyFields.ToArray();
                fields = fields.Where(fieldDef =>
                    fieldsArray.Contains(_dialectProvider.GetQuotedColumnName(fieldDef.Name)));
            }

            //Filters fields not in the T object
            if (typeof(T) != values.GetType())
            {
                var tmp = _modelDefinition.FieldDefinitions.Select(x => x.Name).ToArray();
                fields = fields.Where(f => tmp.Contains(f.Name));
            }

            //Retrieve new values
            foreach (var fieldDef in fields)
            {
                var pname = _dialectProvider.GetParameterName(Statement.Parameters.Count);
                Statement.UpdateFields.Add(_dialectProvider.GetQuotedColumnName(fieldDef.FieldName), pname);
                Statement.Parameters.Add(new StatementParameter(pname,fieldDef.FieldType, fieldDef.GetValue(values)));
            }

            //Add Primarykey filter if required
            if (addPrimaryKeyWhereCondition)
            {
                var pks = _modelDefinition.FieldDefinitions.Where(x => x.IsPrimaryKey).ToArray();
                foreach (var pk in pks)
                {
                    if (Statement.WhereExpression.Length > 0)
                    {
                        Statement.WhereExpression.Append(" AND ");
                    }

                    var pname = _dialectProvider.GetParameterName(Statement.Parameters.Count);
                    Statement.WhereExpression.Append(_dialectProvider.GetQuotedColumnName(pk.FieldName));
                    Statement.WhereExpression.Append("=");
                    Statement.WhereExpression.Append(pname);
                    Statement.Parameters.Add(new StatementParameter(pname,pk.FieldType, pk.GetValue(values)));
                }
            }

            return this;
        }

        protected override WhereStatement GetWhereStatement()
        {
            return Statement;
        }

        /// <summary>Wheres the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public new TypedUpdateStatement<T> Where(Expression<Func<T, bool>> predicate)
        {
            base.And(predicate);
            return this;
        }

        /// <summary>Ands the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public new TypedUpdateStatement<T> And(Expression<Func<T, bool>> predicate)
        {
            base.Where(GetExpressionVisitor(), predicate);
            return this;
        }


        /// <summary>Ors the given predicate.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
        public new TypedUpdateStatement<T> Or(Expression<Func<T, bool>> predicate)
        {
            base.Where(GetExpressionVisitor(), predicate, "OR");
            return this;
        }

        private TableWhereExpresionVisitor<T> GetExpressionVisitor()
        {
            return new TableWhereExpresionVisitor<T>(_dialectProvider, Statement.Parameters, _modelDefinition);
        }

        private TableWFieldsExpresionVisitor<T> GetFieldsExpressionVisitor(bool addAliasSpecification = false)
        {
            return new TableWFieldsExpresionVisitor<T>(_dialectProvider, Statement.Parameters, _modelDefinition,
                addAliasSpecification);
        }
    }
}