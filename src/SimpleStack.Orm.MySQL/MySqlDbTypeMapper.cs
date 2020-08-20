using System;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace SimpleStack.Orm.MySQL
{
    public class MySqlDbTypeMapper : DbTypeMapperBase
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
                case DbType.Xml:
                    if (l < 10000)
                    {
                        return $"VARCHAR({l})";
                    }

                    return l < 65535 ? "TEXT" : "LONGTEXT";
                case DbType.Binary:
                case DbType.Object:
                    return "BLOB";
                case DbType.Boolean:
                    return "BOOLEAN";
                case DbType.SByte:
                case DbType.Byte:
                    return "TINYINT";
                case DbType.Int16:
                    return "SMALLINT";
                case DbType.UInt16:
                    return "SMALLINT UNSIGNED";
                case DbType.Int32:
                case DbType.VarNumeric:
                    return "INT";
                case DbType.UInt32:
                    return "INT UNSIGNED";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.UInt64:
                    return "BIGINT UNSIGNED";
                case DbType.Single:
                    return "FLOAT";
                case DbType.Double:
                    return "DOUBLE";
                case DbType.Decimal:
                case DbType.Currency:
                    return $"DECIMAL({precision ?? DefaultPrecision},{scale ?? DefaultScale})";
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "DATETIME";
                case DbType.Time:
                    return "TIME";
                case DbType.Guid:
                    return
                        "CHAR(36)"; //BINARY(16) https://dev.mysql.com/doc/connector-net/en/connector-net-6-10-connection-options.html
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}