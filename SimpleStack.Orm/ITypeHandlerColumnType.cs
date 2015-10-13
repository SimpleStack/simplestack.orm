using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SimpleStack.Orm
{
	interface ITypeHandlerColumnType : SqlMapper.ITypeHandler
	{
		int? Length { get; }

		DbType ColumnType { get; } 
	}
}
