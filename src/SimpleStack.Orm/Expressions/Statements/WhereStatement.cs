using System.Text;

namespace SimpleStack.Orm.Expressions.Statements
{
    public abstract class WhereStatement : Statement
    {
        public StringBuilder WhereExpression { get; } = new StringBuilder();

        public virtual void Clear()
        {
            WhereExpression.Clear();
        }
    }
}