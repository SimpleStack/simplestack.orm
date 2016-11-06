using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Attribute for automatic increment.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class AutoIncrementAttribute : Attribute
	{
	}
}