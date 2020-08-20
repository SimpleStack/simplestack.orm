//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Reflection;

namespace SimpleStack.Orm
{
    /// <summary>A field definition.</summary>
    public class FieldDefinition
    {
        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the alias.</summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>Gets the name of the field.</summary>
        /// <value>The name of the field.</value>
        public string FieldName => Alias ?? Name;

        /// <summary>Gets or sets the type of the field.</summary>
        /// <value>The type of the field.</value>
        public Type FieldType { get; set; }

        /// <summary>Gets or sets information describing the property.</summary>
        /// <value>Information describing the property.</value>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is primary key.</summary>
        /// <value>true if this object is primary key, false if not.</value>
        public bool IsPrimaryKey { get; set; }

        /// <summary>Gets or sets a value indicating whether the automatic increment.</summary>
        /// <value>true if automatic increment, false if not.</value>
        public bool AutoIncrement { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is nullable.</summary>
        /// <value>true if this object is nullable, false if not.</value>
        public bool IsNullable { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is indexed.</summary>
        /// <value>true if this object is indexed, false if not.</value>
        public bool IsIndexed { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is unique.</summary>
        /// <value>true if this object is unique, false if not.</value>
        public bool IsUnique { get; set; }

        /// <summary>Gets or sets the length of the field.</summary>
        /// <value>The length of the field.</value>
        public int? FieldLength { get; set; } // Precision for Decimal Type

        /// <summary>Gets or sets the scale.</summary>
        /// <value>The scale.</value>
        public int? Scale { get; set; } //  for decimal type

        /// <summary>Gets or sets the default value.</summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; set; }

        /// <summary>Gets or sets the foreign key.</summary>
        /// <value>The foreign key.</value>
        public ForeignKeyConstraint ForeignKey { get; set; }

        /// <summary>Gets or sets the get value function.</summary>
        /// <value>The get value function.</value>
        public PropertyGetterDelegate GetValueFn { get; set; }
        
        /// <summary>Gets or sets the sequence.</summary>
        /// <value>The sequence.</value>
        public string Sequence { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is computed.</summary>
        /// <value>true if this object is computed, false if not.</value>
        public bool IsComputed { get; set; }

        /// <summary>Gets or sets the compute expression.</summary>
        /// <value>The compute expression.</value>
        public string ComputeExpression { get; set; }
        
        /// <summary>Gets a value.</summary>
        /// <param name="onInstance">The on instance.</param>
        /// <returns>The value.</returns>
        public object GetValue(object onInstance)
        {
            return GetValueFn?.Invoke(onInstance);
        }
    }
}