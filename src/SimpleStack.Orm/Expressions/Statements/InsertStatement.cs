using System.Collections.Generic;

namespace SimpleStack.Orm.Expressions.Statements
{
    public class InsertStatement : Statement
    {
        public List<string> InsertFields { get; } = new List<string>();
    }
}