using System;
using System.Collections.Generic;
using System.Data;

namespace SimpleStack.Orm
{
	/// <summary>A database types.</summary>
	public class DbTypes
	{
		/// <summary>The column database type map.</summary>
		public Dictionary<DbType, string> ColumnDbTypeMap = new Dictionary<DbType, string>();

		/// <summary>Sets.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dbType">         Type of the database.</param>
		/// <param name="fieldDefinition">The field definition.</param>
		public void Set(DbType dbType, string fieldDefinition)
		{
			ColumnDbTypeMap[dbType] = fieldDefinition;
		}
	}
}