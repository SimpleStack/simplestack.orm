using System.Collections.Generic;

namespace SimpleStack.Orm.Expressions.Statements
{
    public class UpdateStatement : WhereStatement
    {
        public Dictionary<string, string> UpdateFields { get; } = new Dictionary<string, string>();
    }
}