using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SimpleStack.Orm
{
    public interface IColumnDefinition
    {
        string Name { get; }
        bool Nullable { get; }
        int? Length { get; }
        int? Precision { get; }
        int? Scale { get; }
        string Definition { get; }
        DbType DbType { get; }
        bool PrimaryKey { get; }
        string DefaultValue { get; }
    }

    public class ColumnType
    {
        public int? Length { get; set; }
        
        public int? Precision { get; set; }
        
        public int? Scale { get; set; }
        
        public string Definition { get; set; }
        
        public DbType DbType { get; set; }
    }

    public class ColumnDefinition : ColumnType, IColumnDefinition
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public bool Nullable { get; set; }

        /// <inheritdoc />
        public bool PrimaryKey { get; set; }

        /// <inheritdoc />
        public string DefaultValue { get; set; }
    }
}
