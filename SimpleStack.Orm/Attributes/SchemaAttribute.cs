using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Used to annotate an Entity with its DB schema.</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class SchemaAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.SchemaAttribute class.
		/// </summary>
		/// <param name="name">The name.</param>
		public SchemaAttribute(string name)
		{
			this.Name = name;
		}

		/// <summary>Gets or sets the name.</summary>
		/// <value>The name.</value>
		public string Name { get; set; }
	}
}