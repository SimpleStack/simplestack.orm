using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStack.Orm
{
    public class ColumnDefinition
    {
        public string Name { get; internal set; }
        public bool Nullable { get; internal set; }
        public int FieldLength { get; internal set; }
        public string Type { get; internal set; }
        public bool PrimaryKey { get; internal set; }
        public string DefaultValue { get; internal set; }
    }
}
