using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions
{
    internal abstract class ExpressionVisitor
    {
        protected readonly IDialectProvider DialectProvider;
        protected readonly IDictionary<string, object> Parameters;

        protected ExpressionVisitor(IDialectProvider dialectProvider,
            IDictionary<string, object> parameters)
        {
            DialectProvider = dialectProvider;
            Parameters = parameters;
        }

        public virtual string VisitExpression(Expression exp)
        {
            var statement = Visit(exp);
            
            if (statement == null)
                return string.Empty;
            
            if (statement is ParameterPart pp) 
                return (bool) Parameters[pp.Text] ? "1=1" : "1=0";
            
            if (statement is ColumnAccessPart cp) 
                return $"{cp.Text} = {AddParameter(true)}";
            
            return statement.ToString();
        }

        protected StatementPart Visit(Expression exp)
        {
            if (exp == null) return null;
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary(exp as BinaryExpression);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit(exp as MemberInitExpression);
                default:
                    return new StatementPart(exp.ToString());
            }
        }

        protected abstract bool IsColumnAccess(MethodCallExpression methodCallExpression);

        protected virtual StatementPart VisitParameter(ParameterExpression parameterExpression)
        {
            return null;
        }

        protected virtual StatementPart VisitNewArray(NewArrayExpression newArrayExpression)
        {
            return null;
        }

        protected virtual StatementPart VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            return null;
        }

        protected virtual StatementPart VisitNew(NewExpression newExpression)
        {
            return null;
        }

        protected virtual StatementPart VisitMethodCall(MethodCallExpression m)
        {
            if (IsArrayMethod(m))
                return VisitArrayMethodCall(m);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m);

            var value = Expression.Lambda(m).Compile().DynamicInvoke();
            if (value == null)
                return null;
            return AddParameter(value);
        }

        protected virtual StatementPart VisitColumnAccessMethod(MethodCallExpression m)
        {
            var args = new List<StatementPart>();
            if (m.Arguments.Count == 1 && m.Arguments[0].NodeType == ExpressionType.Constant)
                args.Add(VisitConstant((ConstantExpression) m.Arguments[0]));
            else
                args.AddRange(VisitExpressionList(m.Arguments));

            var quotedColName = Visit(m.Object).ToString();
            var statement = DialectProvider.GetStringFunction(
                m.Method.Name,
                quotedColName,
                Parameters,
                args.Select(x => x.Text).ToArray());

            return new StatementPart(statement);
        }

        protected virtual bool IsArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
                if (m.Arguments.Count == 2)
                    return true;

            return false;
        }

        protected virtual StatementPart VisitArrayMethodCall(MethodCallExpression m)
        {
            return null;
        }

        protected virtual List<StatementPart> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<StatementPart>();
            for (int i = 0, n = original.Count; i < n; i++)
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                    original[i].NodeType == ExpressionType.NewArrayBounds)
                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                else
                    list.Add(Visit(original[i]));
            return list;
        }

        protected virtual List<StatementPart> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }

        protected virtual StatementPart VisitUnary(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    var o = Visit(unaryExpression.Operand);

                    if (o is ParameterPart op && op.ParameterType == typeof(bool))
                    {
                        Parameters[op.Text] = !(bool) Parameters[op.Text];
                        return o;
                    }

                    if (o is ColumnAccessPart oc && oc.ColumnType == typeof(bool))
                        o = new StatementPart($"{o.Text} = {AddParameter(true)}");

                    return new StatementPart($"NOT ({o})");

                case ExpressionType.Convert:
                    if (unaryExpression.Method != null)
                    {
                        var value = Expression.Lambda(unaryExpression).Compile().DynamicInvoke();
                        if (value != null) return AddParameter(value);
                        return null;
                    }

                    break;
            }

            return Visit(unaryExpression.Operand);
        }

        protected virtual StatementPart VisitBinary(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);

            var leftca = left as ColumnAccessPart;
            var rightca = right as ColumnAccessPart;

            var leftp = left as ParameterPart;
            var rightp = right as ParameterPart;

            if (leftp != null && rightp != null)
            {
                // if both sides are parameters, let's do the comparison here
                Parameters.Remove(leftp.Text);
                Parameters.Remove(rightp.Text);

                var result = Expression.Lambda(binaryExpression).Compile().DynamicInvoke();
                return AddParameter(result);
            }

            var isIntegral = leftp != null && leftp.ParameterType != typeof(bool);

            if (!isIntegral && rightp != null && rightp.ParameterType != typeof(bool))
            {
                isIntegral = true;
            }

            var operand = DialectProvider.BindOperand(binaryExpression.NodeType, isIntegral);


            if (operand == "AND" || operand == "OR")
            {
                if (leftca?.ColumnType == typeof(bool))
                    left = new StatementPart($"{leftca.Text} = {AddParameter(true)}");

                if (rightca?.ColumnType == typeof(bool))
                    right = new StatementPart($"{rightca.Text} = {AddParameter(true)}");

                if (leftp?.ParameterType == typeof(bool) || rightp?.ParameterType == typeof(bool))
                {
                    var boolValue = (bool) (leftp != null ? Parameters[leftp.Text] : Parameters[rightp.Text]);
                    if (operand == "AND")
                    {
                        if (boolValue) return leftp != null ? left : right;

                        return AddParameter(false);
                    }

                    if (operand == "OR")
                    {
                        if (boolValue) return AddParameter(true);

                        return leftp != null ? left : right;
                    }
                }
            }
            else
            {
                if (leftca != null && leftca.ColumnType.IsEnum() && rightp != null)
                    Parameters[rightp.Text] = Enum.ToObject(leftca.ColumnType, Parameters[rightp.Text]);

                if (rightca != null && rightca.ColumnType.IsEnum() && leftp != null)
                    Parameters[leftp.Text] = Enum.ToObject(rightca.ColumnType, Parameters[leftp.Text]);
            }

            if (right == null || left == null) operand = operand == "=" ? "IS" : "IS NOT";

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new StatementPart($"{operand}({left},{right})");
                default:
                    return new StatementPart((left == null ? "NULL" : left.ToString()) + " " + operand + " " +
                                             (right == null ? "NULL" : right.ToString()));
            }
        }

        protected virtual StatementPart VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value != null) return AddParameter(constantExpression.Value);
            return null;
        }

        protected virtual StatementPart VisitMemberAccess(MemberExpression memberExpression)
        {
            var r = Expression.Lambda(memberExpression).Compile().DynamicInvoke();
            if (r != null)
                return AddParameter(r);

            return null;
        }

        protected virtual StatementPart VisitLambda(LambdaExpression lambdaExpression)
        {
            return Visit(lambdaExpression.Body);
        }

        protected virtual StatementPart VisitDateTimeMemberAccess(MemberExpression m)
        {
            var quotedColName = Visit(m.Expression);

            string statement;

            switch (m.Member.Name)
            {
                case "Year":
                case "Month":
                case "Day":
                case "Hour":
                case "Minute":
                case "Second":
                    statement = DialectProvider.GetDatePartFunction(m.Member.Name, quotedColName.ToString());
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new StatementPart(statement);
        }

        /// <summary>
        ///     Add a parameter to the query and return corresponding parameter name
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected ParameterPart AddParameter(object param)
        {
            var paramName = DialectProvider.GetParameterName(Parameters.Count);
            Parameters.Add(paramName, param);
            return new ParameterPart(paramName, param.GetType());
        }

        protected class StatementPart
        {
            public StatementPart(string text)
            {
                Text = text;
            }

            public string Text { get; }

            public override string ToString()
            {
                return Text;
            }
        }

        protected class ParameterPart : StatementPart
        {
            public ParameterPart(string parameterName, Type parameterType)
                : base(parameterName)
            {
                ParameterType = parameterType;
            }

            public Type ParameterType { get; }
        }

        protected class ColumnAccessPart : StatementPart
        {
            public ColumnAccessPart(string columnName, Type columnType)
                : base(columnName)
            {
                ColumnType = columnType;
            }

            public Type ColumnType { get; }
        }
    }
}