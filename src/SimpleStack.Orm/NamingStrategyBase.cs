//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//   Tomasz Kubacki (tomasz.kubacki@gmail.com)
//
// Copyright 2012 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

namespace SimpleStack.Orm
{
    /// <summary>An ORM lite naming strategy base.</summary>
	public class NamingStrategyBase : INamingStrategy
	{
        /// <summary>Gets table name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The table name.</returns>
		public virtual string GetTableName(string name)
		{
			return name;
		}

        /// <summary>Gets column name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The column name.</returns>
		public virtual string GetColumnName(string name)
		{
			return name;
		}
	}
}
