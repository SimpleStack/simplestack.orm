using System;
using System.Data;

namespace SimpleStack.Orm.PostgreSQL
{
    public class PostgreSQLTypeMapper : DbTypeMapperBase
    {
        public override string GetFieldDefinition(DbType type, int? length = null, int? scale = null,
            int? precision = null)
        {
            var l = length ?? DefaultStringLength;
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    //TODO: for performance reason, text is better all the time, add a option to use text all the time
                    return length != null ? $"character varying({length.Value})" : "text";
                case DbType.Binary:
                case DbType.Object:
                    return "bytea";
                case DbType.Boolean:
                    return "boolean";
                case DbType.SByte:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.UInt16:
                    return "smallint";
                case DbType.Int32:
                case DbType.UInt32:
                case DbType.VarNumeric:
                    return "integer";
                case DbType.Int64:
                case DbType.UInt64:
                    return "bigint";
                case DbType.Single:
                    return "real";
                case DbType.Double:
                    return "double precision";
                case DbType.Decimal:
                    return $"decimal({precision ?? DefaultPrecision},{scale ?? DefaultScale})";
                case DbType.Currency:
                    return "money";
                case DbType.Date:
                    return "date";
                case DbType.DateTime:
                case DbType.DateTime2:
                    return "timestamp";
                case DbType.DateTimeOffset:
                    return "timestamp with timezone";
                case DbType.Time:
                    return "Interval";
                case DbType.Guid:
                    return "uuid";
                case DbType.Xml:
                    return "xml";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}