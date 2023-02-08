using System.Linq;

namespace SimpleStack.Orm.Expressions.Statements.Typed
{
    public class TypedDeleteStatement<T> : TypedWhereStatement<T>
    {
        private readonly IDialectProvider _dialectProvider;

        private readonly ModelDefinition _modelDefinition = typeof(T).GetModelDefinition();

        public TypedDeleteStatement(IDialectProvider dialectProvider)
            : base(dialectProvider)
        {
            _dialectProvider = dialectProvider;
        }

        public DeleteStatement Statement { get; } = new DeleteStatement();

        public TypedDeleteStatement<T> AddPrimaryKeyWhereCondition(T values)
        {
            var pks = _modelDefinition.FieldDefinitions.Where(x => x.IsPrimaryKey).ToArray();

            if (pks.Length == 0)
            {
                pks = _modelDefinition.FieldDefinitions.ToArray();
            }

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

            return this;
        }

        protected override WhereStatement GetWhereStatement()
        {
            return Statement;
        }
    }
}