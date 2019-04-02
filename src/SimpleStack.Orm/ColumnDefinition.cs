using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStack.Orm
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public int Character_Length { get; set; }
        public string Type { get; set; }
        public bool PrimaryKey { get; set; }
        public string DefaultValue { get; set; }
    }
}
