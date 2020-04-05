using System.Data;
using Dapper;

namespace SimpleStack.Orm
{
    public interface ITypeHandlerColumnType : SqlMapper.ITypeHandler
    {
        int? Length { get; }

        DbType ColumnType { get; }
    }
}