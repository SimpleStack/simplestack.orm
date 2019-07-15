using System.Collections.Generic;

namespace SimpleStack.Orm.Expressions.Statements
{
    public abstract class Statement
    {
        public string TableName { get; set; }
        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    }
}