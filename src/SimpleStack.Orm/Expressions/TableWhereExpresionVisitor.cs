using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleStack.Orm.Expressions.Statements;

namespace SimpleStack.Orm.Expressions
{
    internal class TableWhereExpresionVisitor<T> : ExpressionVisitor
    {
        private readonly ModelDefinition _modelDefinition;

        public TableWhereExpresionVisitor(IDialectProvider dialectProvider,
            StatementParameters parameters,
            ModelDefinition modelDefinition)
            : base(dialectProvider, parameters)
        {
            _modelDefinition = modelDefinition;
        }

        protected override StatementPart VisitMemberAccess(MemberExpression m)
        {
            //Nullable support HasValue
            if (m.Member.DeclaringType != null && 
                Nullable.GetUnderlyingType(m.Member.DeclaringType) != null &&
                m.Member.Name == "HasValue")
            {
                return new StatementPart(Visit(m.Expression) + " IS NOT NULL");
            }
            
            if (m.Member.DeclaringType == typeof(string) &&
                m.Member.Name == "Length")
            {
                return new StatementPart(
                    DialectProvider.GetStringFunction("length",
                        Visit(m.Expression).ToString(),
                        null,
                        null));
            }
            
            if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTime?))
            {
                return VisitDateTimeMemberAccess(m);
            }

            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter ||
                                         m.Expression.NodeType == ExpressionType.Convert))
            {
                var propertyInfo = m.Member as PropertyInfo;
                return new ColumnAccessPart(GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType);
            }

            var r = Expression.Lambda(m).Compile().DynamicInvoke();
            return r != null ? AddParameter(r) : null;
        }

        protected override bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object is MethodCallExpression mce)
            {
                return IsColumnAccess(mce);
            }

            var exp = m.Object as MemberExpression;
            return exp?.Expression != null &&
                   exp.Expression.Type == typeof(T) &&
                   exp.Expression.NodeType == ExpressionType.Parameter;
        }

        protected override StatementPart VisitArrayMethodCall(MethodCallExpression m)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    var args = VisitExpressionList(m.Arguments.ToArray());
                    var quotedColName = args.Last(x => x is ColumnAccessPart);

                    IEnumerable<StatementPart> parameters;
                    parameters = m.Object == null
                        ? args.TakeWhile(x => x is ParameterPart)
                        : VisitExpressionList(new[] {m.Object});

                    var sIn = parameters.Select(x => x.Text).Aggregate((x, y) => x + "," + y);

                    statement = $"{quotedColName} IN ({(string.IsNullOrEmpty(sIn) ? "NULL" : sIn)})";
                    break;

                default:
                    throw new NotSupportedException($"Method '{m.Method.Name}' not supported");
            }

            return new StatementPart(statement);
        }

        protected virtual string GetQuotedColumnName(string memberName)
        {
            var fd = _modelDefinition.FieldDefinitions.FirstOrDefault(x => x.Name.ToLower() == memberName.ToLower());
            if (fd == null)
            {
                throw new OrmException($"Column name '{memberName}' not found in type '{_modelDefinition.Name}'");
            }

            return fd.IsComputed ? fd.ComputeExpression : DialectProvider.GetQuotedColumnName(fd.FieldName);
        }
    }
}