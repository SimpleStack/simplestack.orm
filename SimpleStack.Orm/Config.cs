using System;
using System.Data;

namespace SimpleStack.Orm
{
	public static class Config
	{
		/// <summary>The ts transaction.</summary>
		[ThreadStatic]
		internal static OrmLiteTransaction TSTransaction;

		static Config()
		{
			IdField = "Id";
		}

		public static IDialectProvider DialectProvider { get;set;}

		public static string IdField { get; set; }
		public static int CommandTimeout { get; set; }
	}
}