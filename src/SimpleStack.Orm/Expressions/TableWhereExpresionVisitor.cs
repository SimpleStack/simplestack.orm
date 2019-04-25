using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleStack.Orm.Expressions
{
    internal class TableWhereExpresionVisitor<T> : ExpressionVisitor
    {
        private readonly ModelDefinition _modelDefinition;

        public TableWhereExpresionVisitor(IDialectProvider dialectProvider,
            IDictionary<string, object> parameters,
            ModelDefinition modelDefinition)
            : base(dialectProvider, parameters)
        {
            _modelDefinition = modelDefinition;
        }

        protected override StatementPart VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(DateTime))
                return VisitDateTimeMemberAccess(m);

            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter ||
                                         m.Expression.NodeType == ExpressionType.Convert))
            {
                var propertyInfo = m.Member as PropertyInfo;
                return new ColumnAccessPart(GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType);
            }

            var r = Expression.Lambda(m).Compile().DynamicInvoke();
            if (r != null)
                return AddParameter(r);

            return null;
        }

        protected override bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object is MethodCallExpression)
                return IsColumnAccess((MethodCallExpression) m.Object);

            var exp = m.Object as MemberExpression;
            return exp?.Expression != null && exp.Expression.Type == typeof(T) &&
                   exp.Expression.NodeType == ExpressionType.Parameter;
        }

        protected override StatementPart VisitArrayMethodCall(MethodCallExpression m)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    var args = VisitExpressionList(m.Arguments);
                    var quotedColName = args.Last(x => x is ColumnAccessPart);

                    var sIn = args.TakeWhile(x => x is ParameterPart)
                        .Select(x => x.Text)
                        .Aggregate((x, y) => x + "," + y);

                    statement = string.Format("{0} {1} ({2})", quotedColName, "In",
                        string.IsNullOrEmpty(sIn) ? "NULL" : sIn);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new StatementPart(statement);
        }

        protected virtual string GetQuotedColumnName(string memberName)
        {
            var fd = _modelDefinition.FieldDefinitions.FirstOrDefault(x => x.Name == memberName);
            var fn = fd?.FieldName ?? memberName;

            return DialectProvider.GetQuotedColumnName(fn);
        }
    }
}