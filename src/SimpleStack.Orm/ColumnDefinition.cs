using System.Data;

namespace SimpleStack.Orm
{
    public interface IColumnDefinition
    {
        /// <summary>
        /// Column Name
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Is nullable or not
        /// </summary>
        bool Nullable { get; }
        
        /// <summary>
        /// Column Length
        /// </summary>
        int? Length { get; }
        
        /// <summary>
        /// Numeric precision
        /// </summary>
        int? Precision { get; }
        
        /// <summary>
        /// Numeric Scale
        /// </summary>
        int? Scale { get; }
        
        /// <summary>
        /// Columns definition as returned by database server
        /// </summary>
        string Definition { get; }
        
        /// <summary>
        /// Columns type
        /// </summary>
        DbType DbType { get; }
        
        /// <summary>
        /// True if column is part of the primary key
        /// </summary>
        bool PrimaryKey { get; }
        
        /// <summary>
        /// True if there is a unique index on that column
        /// </summary>
        bool Unique { get; }
        
        /// <summary>
        /// Column default value
        /// </summary>
        string DefaultValue { get; }
        
        /// <summary>
        /// Computed Column Expression
        /// </summary>
        string ComputedExpression { get; }
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

        public bool Unique { get; set; }

        /// <inheritdoc />
        public string DefaultValue { get; set; }
        
        /// <inheritdoc />
        public string ComputedExpression { get; set; }
    }
}