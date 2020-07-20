using System.Data;

namespace SimpleStack.Orm
{
    public interface IDbTypeMapper
    {
        /// <summary>
        /// Default string length in database if <see cref="SimpleStack.Orm.Attributes.StringLengthAttribute"/> is not used
        /// </summary>
        int DefaultStringLength { get; set; }
        
        /// <summary>
        /// Specify if Unicode must be used to store string in database
        /// </summary>
        bool UseUnicode { get; set; }
        
        /// <summary>
        /// Default decimal precision if <see cref="SimpleStack.Orm.Attributes.DecimalLengthAttribute"/> is not used
        /// </summary>
        int DefaultPrecision { get; set; }
        
        /// <summary>
        /// Default decimal scale if <see cref="SimpleStack.Orm.Attributes.DecimalLengthAttribute"/> is not used
        /// </summary>
        int DefaultScale { get; set; }
        
        string GetFieldDefinition(DbType type, int? length = null, int? scale = null, int? precision = null);
    }

    public abstract class DbTypeMapperBase : IDbTypeMapper
    {
        public int DefaultStringLength { get; set; } = 255;

        public bool UseUnicode { get; set; } = true;
        
        public int DefaultPrecision { get; set; } = 38;
        
        public int DefaultScale { get; set; } = 6;

        public abstract string GetFieldDefinition(
            DbType type,
            int? length = null,
            int? scale = null,
            int? precision = null);
    }
}