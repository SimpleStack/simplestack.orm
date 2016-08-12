using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Compute attribute. Use to indicate that a property is a Calculated Field.</summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ComputeAttribute : Attribute
	{
		/// <summary>Gets or sets the expression.</summary>
		/// <value>The expression.</value>
		public string Expression { get; set; }

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.ComputeAttribute class.
		/// </summary>
		public ComputeAttribute()
			: this(string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.ComputeAttribute class.
		/// </summary>
		/// <param name="expression">The expression.</param>
		public ComputeAttribute(string expression)
		{
			Expression = expression;
		}
	}
}