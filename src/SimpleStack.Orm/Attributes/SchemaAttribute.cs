using System;

namespace SimpleStack.Orm.Attributes
{
    /// <summary>Used to annotate an Entity with its DB schema.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SchemaAttribute : Attribute
    {
	    /// <summary>
	    ///     Initializes a new instance of the SchemaAttribute class.
	    /// </summary>
	    /// <param name="name">The name.</param>
	    public SchemaAttribute(string name)
        {
            Name = name;
        }

        /// <summary>Gets or sets the schema name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}