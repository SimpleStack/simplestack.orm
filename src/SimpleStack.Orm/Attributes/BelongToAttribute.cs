using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>
	/// BelongToAttribute Use to indicate that a join column belongs to another table.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class BelongToAttribute : Attribute
	{
		/// <summary>Gets or sets the type of the belong to table.</summary>
		/// <value>The type of the belong to table.</value>
		public Type BelongToTableType { get; set; }

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.BelongToAttribute class.
		/// </summary>
		/// <param name="belongToTableType">Type of the belong to table.</param>
		public BelongToAttribute(Type belongToTableType)
		{
			BelongToTableType = belongToTableType;
		}
	}
}