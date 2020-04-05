using System;

namespace SimpleStack.Orm.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// Sequence attribute.
    /// Use in FirebirdSql. indicates name of generator for columns of type AutoIncrement
    // </summary>
    /// <summary>Attribute for sequence.</summary>
    public class SequenceAttribute : Attribute
    {
	    /// <summary>
	    ///     Initializes a new instance of the NServiceKit.DataAnnotations.SequenceAttribute class.
	    /// </summary>
	    /// <param name="name">The name.</param>
	    public SequenceAttribute(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}