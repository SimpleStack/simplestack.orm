using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions
{
    internal class ColumnWhereExpresionVisitor<T> : ExpressionVisitor
    {
        private readonly string _statement;

        public ColumnWhereExpresionVisitor(IDialectProvider dialectProvider,
            IDictionary<string, object> parameters,
            string statement)
            : base(dialectProvider, parameters)
        {
            _statement = statement;
        }

        protected override StatementPart VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Sql))
                return VisitSqlMethodCall(methodCallExpression);

            if (IsColumnAccess(methodCallExpression))
                return VisitColumnMethodCall(methodCallExpression);

            if (IsArrayMethod(methodCallExpression))
                return VisitArrayMethodCall(methodCallExpression);

            var value = Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();
            if (value != null)
                return AddParameter(value);
            return null;
        }

        protected override bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object is MethodCallExpression)
                return IsColumnAccess((MethodCallExpression) m.Object);

            return m.Object?.NodeType == ExpressionType.Parameter;
        }

        protected override StatementPart VisitArrayMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    var memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = m.Arguments[0] as MemberExpression;

                    if (memberExpr != null)
                    {
                        var lambda = Expression.Lambda<Func<IEnumerable<T>>>(memberExpr).Compile();
                        var inArgs = lambda();
                        var sIn = inArgs.Select(x => AddParameter(x)).Select(x => x.Text)
                            .Aggregate((x, y) => x + "," + y);

                        if (sIn.Length == 0)
                            // The collection is empty, so avoid generating invalid SQL syntax of "ColumnName IN ()".
                            // Instead, just select from the null set via "ColumnName IN (NULL)"
                            sIn = "NULL";

                        return new StatementPart($"{_statement} IN ({sIn})");
                    }

                    break;
            }

            throw new NotSupportedException();
        }

        protected virtual StatementPart VisitColumnMemberAccess(MemberExpression memberExpression)
        {
            if (memberExpression.Member.DeclaringType == typeof(string) &&
                memberExpression.Member.Name == "Length")
                return new StatementPart(
                    DialectProvider.GetStringFunction("length",
                        _statement,
                        null,
                        null));

            if (memberExpression.Member.DeclaringType == typeof(DateTime))
                switch (memberExpression.Member.Name)
                {
                    case "Year":
                    case "Month":
                    case "Day":
                    case "Hour":
                    case "Minute":
                    case "Second":
                        return new StatementPart(
                            DialectProvider.GetDatePartFunction(memberExpression.Member.Name, _statement));
                    default:
                        throw new NotSupportedException();
                }

            return null;
        }

        protected virtual StatementPart VisitSqlMethodCall(MethodCallExpression m)
        {
            var args = VisitExpressionList(m.Arguments);

            switch (m.Method.Name)
            {
                case "As":
                    //the columnName has been added as a parameter, we have to remove it;
                    var asName = Parameters[args[1].ToString()].ToString();
                    Parameters.Remove(args[1].ToString());
                    return new StatementPart($"{args[0]} As {DialectProvider.GetQuotedColumnName(asName)}");
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                    return new StatementPart($"{m.Method.Name}({args[0]})");
            }

            throw new NotSupportedException();
        }

        protected virtual StatementPart VisitColumnMethodCall(MethodCallExpression methodCallExpression)
        {
            var args = new List<StatementPart>();
            if (methodCallExpression.Arguments.Count == 1 &&
                methodCallExpression.Arguments[0].NodeType == ExpressionType.Constant)
                args.Add(VisitConstant((ConstantExpression) methodCallExpression.Arguments[0]));
            else
                args.AddRange(VisitExpressionList(methodCallExpression.Arguments));

            var quotedColName = Visit(methodCallExpression.Object).ToString();

            var statement = DialectProvider.GetStringFunction(
                methodCallExpression.Method.Name,
                quotedColName,
                Parameters,
                args.Select(x => x.Text).ToArray());

            return new StatementPart(statement);
        }

        protected override StatementPart VisitParameter(ParameterExpression parameterExpression)
        {
            return /* tableName.*/ new ColumnAccessPart(_statement, typeof(T));
        }

        protected override StatementPart VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value != null) return AddParameter(constantExpression.Value);
            return null;
        }

        protected override StatementPart VisitMemberAccess(MemberExpression memberExpression)
        {
            if (memberExpression.Expression?.NodeType == ExpressionType.Parameter)
                return VisitColumnMemberAccess(memberExpression);

            var r = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
            if (r != null)
                return AddParameter(r);

            return null;
        }
    }
}