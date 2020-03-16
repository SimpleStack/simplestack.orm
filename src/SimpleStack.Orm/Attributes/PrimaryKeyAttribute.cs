using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Primary key attribute. use to indicate that property is part of the pk.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PrimaryKeyAttribute : Attribute
	{

	}
	
}