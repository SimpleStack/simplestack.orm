using System;

namespace SimpleStack.Orm.Attributes
{
	/// <summary>Decimal length attribute.</summary>	
	[AttributeUsage(AttributeTargets.Property)]
	public class DecimalLengthAttribute : Attribute
	{
		/// <summary>Gets or sets the precision.</summary>
		/// <value>The precision.</value>
		public int Precision { get; set; }

		/// <summary>Gets or sets the scale.</summary>
		/// <value>The scale.</value>
		public int Scale { get; set; }

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.DecimalLengthAttribute
		/// class.
		/// </summary>
		/// <param name="precision">The precision.</param>
		/// <param name="scale">    The scale.</param>
		public DecimalLengthAttribute(int precision, int scale)
		{
			Precision = precision;
			Scale = scale;
		}

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.DecimalLengthAttribute
		/// class.
		/// </summary>
		/// <param name="precision">The precision.</param>
		public DecimalLengthAttribute(int precision)
			: this(precision, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the NServiceKit.DataAnnotations.DecimalLengthAttribute
		/// class.
		/// </summary>
		public DecimalLengthAttribute()
			: this(18, 0)
		{
		}

	}
}