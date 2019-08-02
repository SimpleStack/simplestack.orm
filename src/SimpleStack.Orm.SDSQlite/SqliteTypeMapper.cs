using System;
using System.Data;

namespace SimpleStack.Orm.SDSQlite
{
    public class SqliteTypeMapper : DbTypeMapperBase
    {
        public override string GetFieldDefinition(DbType type, int? length = null, int? scale = null, int? precision = null)
        {
            var l = length ?? DefaultStringLength;
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                case DbType.Xml:
                    return "text";// : $"character varying({l})";
                case DbType.Guid:
                    return "guid";
                case DbType.Binary:
                case DbType.Object:
                    return "bytea";
                case DbType.Boolean:
                    return "boolean";
                case DbType.SByte:
                case DbType.Byte:
                    return "tinyint";
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
                case DbType.Currency:
                    return $"decimal({precision ?? DefaultPrecision},{scale ?? DefaultScale})";
                case DbType.Date:
                    return "date";
                case DbType.Time:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "datetime";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}