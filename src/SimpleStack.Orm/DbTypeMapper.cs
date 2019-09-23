using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SimpleStack.Orm
{
	public interface IDbTypeMapper
	{
		string GetFieldDefinition(DbType type, int? length = null, int? scale = null, int? precision = null);
		
		int DefaultStringLength { get; set; }
		bool UseUnicode { get; set; }
		int DefaultPrecision { get; set; }
		int DefaultScale { get; set; }
	}

	public abstract class DbTypeMapperBase : IDbTypeMapper
	{
		//protected Regex FieldDefinitionRegex = new Regex(@"(?<type>\w+)(\((?<param1>\d+(\,(?<param2>\d+))?)\))?",RegexOptions.Compiled);
		
		public int DefaultStringLength { get; set; } = 255;

		public bool UseUnicode { get; set; } = true;

		/// <summary>Gets or sets the default decimal precision.</summary>
		/// <value>The default decimal precision.</value>
		public int DefaultPrecision { get; set; } = 38;

		/// <summary>Gets or sets the default decimal scale.</summary>
		/// <value>The default decimal scale.</value>
		public int DefaultScale { get; set; } = 6;

		public abstract string GetFieldDefinition(
			DbType type,
			int? length = null,
			int? scale = null,
			int? precision = null);
	}
}