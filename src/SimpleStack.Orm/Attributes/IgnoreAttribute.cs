using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>
	/// IgnoreAttribute
	/// Use to indicate that a property is not a field  in the table
	/// properties with this attribute are ignored when building sql sentences
	/// </summary>
	/// <summary>Attribute for automatic increment.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class IgnoreAttribute : Attribute
	{
	}
}