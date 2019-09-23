using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Attribute for default.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DefaultAttribute : Attribute
	{
		/// <summary>Gets or sets the default value.</summary>
		///
		/// <value>The default value.</value>
		public object DefaultValue { get; set; }
		
		public DefaultAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}
	}
}