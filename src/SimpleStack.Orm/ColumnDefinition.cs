using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStack.Orm
{
    public interface IColumnDefinition
    {
        string Name { get; }
        bool Nullable { get; }
        int? FieldLength { get; }
        string Type { get; }
        bool PrimaryKey { get; }
        string DefaultValue { get; }
    }

    public class ColumnDefinition : IColumnDefinition
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public bool Nullable { get; set; }

        /// <inheritdoc />
        public int? FieldLength { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public bool PrimaryKey { get; set; }

        /// <inheritdoc />
        public string DefaultValue { get; set; }
    }
}
