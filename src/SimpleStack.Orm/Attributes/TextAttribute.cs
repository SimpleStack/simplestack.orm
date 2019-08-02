using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Text attribute. Use to indicate that property contains a large amount of text.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class TextAttribute : Attribute
	{

	}
}