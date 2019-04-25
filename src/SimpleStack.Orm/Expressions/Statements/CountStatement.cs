using System.Collections.Generic;
using System.Text;

namespace SimpleStack.Orm.Expressions.Statements
{
    public abstract class CountStatement : WhereStatement
    {
        public StringBuilder GroupByExpression { get; } = new StringBuilder();
        public StringBuilder HavingExpression { get; } = new StringBuilder();
        public List<string> Columns { get; } = new List<string>();
        public bool IsDistinct { get; set; }

        public override void Clear()
        {
            base.Clear();

            Columns.Clear();
            GroupByExpression.Clear();
            HavingExpression.Clear();
            IsDistinct = false;
        }
    }
}