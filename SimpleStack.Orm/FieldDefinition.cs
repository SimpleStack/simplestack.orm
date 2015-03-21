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
        public string FieldName
        {
            get { return this.Alias ?? this.Name; }
        }

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
        public int? FieldLength { get; set; }  // Precision for Decimal Type

        /// <summary>Gets or sets the scale.</summary>
        /// <value>The scale.</value>
        public int? Scale { get; set; }  //  for decimal type

        /// <summary>Gets or sets the default value.</summary>
        /// <value>The default value.</value>
        public string DefaultValue { get; set; }

        /// <summary>Gets or sets the foreign key.</summary>
        /// <value>The foreign key.</value>
        public ForeignKeyConstraint ForeignKey { get; set; }

        /// <summary>Gets or sets the get value function.</summary>
        /// <value>The get value function.</value>
        public PropertyGetterDelegate GetValueFn { get; set; }

        /// <summary>Gets or sets the set value function.</summary>
        /// <value>The set value function.</value>
        public PropertySetterDelegate SetValueFn { get; set; }

        /// <summary>Gets a value.</summary>
        /// <param name="onInstance">The on instance.</param>
        /// <returns>The value.</returns>
        public object GetValue(object onInstance)
        {
            return this.GetValueFn == null ? null : this.GetValueFn(onInstance);
        }

        /// <summary>Gets quoted value.</summary>
        /// <param name="fromInstance">from instance.</param>
        /// <returns>The quoted value.</returns>
        public string GetQuotedValue(object fromInstance)
        {
            var value = GetValue(fromInstance);
            return Config.DialectProvider.GetQuotedValue(value, FieldType);
        }

        /// <summary>Gets or sets the sequence.</summary>
        /// <value>The sequence.</value>
        public string Sequence { get; set; }

        /// <summary>Gets or sets a value indicating whether this object is computed.</summary>
        /// <value>true if this object is computed, false if not.</value>
        public bool IsComputed { get; set; }

        /// <summary>Gets or sets the compute expression.</summary>
        /// <value>The compute expression.</value>
        public string ComputeExpression { get; set; }

        /// <summary>Gets or sets the name of the belong to model.</summary>
        /// <value>The name of the belong to model.</value>
        public string BelongToModelName { get; set; }
    }

    /// <summary>A foreign key constraint.</summary>
    public class ForeignKeyConstraint
    {
        /// <summary>
        /// Initializes a new instance of the NServiceKit.OrmLite.ForeignKeyConstraint class.
        /// </summary>
        /// <param name="type">          The type.</param>
        /// <param name="onDelete">      The on delete.</param>
        /// <param name="onUpdate">      The on update.</param>
        /// <param name="foreignKeyName">The name of the foreign key.</param>
        public ForeignKeyConstraint(Type type, string onDelete = null, string onUpdate = null, string foreignKeyName = null)
        {
            ReferenceType = type;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
            ForeignKeyName = foreignKeyName;
        }

        /// <summary>Gets the type of the reference.</summary>
        /// <value>The type of the reference.</value>
        public Type ReferenceType { get; private set; }

        /// <summary>Gets the on delete.</summary>
        /// <value>The on delete.</value>
        public string OnDelete { get; private set; }

        /// <summary>Gets the on update.</summary>
        /// <value>The on update.</value>
        public string OnUpdate { get; private set; }

        /// <summary>Gets the name of the foreign key.</summary>
        /// <value>The name of the foreign key.</value>
        public string ForeignKeyName { get; private set; }

        /// <summary>Gets foreign key name.</summary>
        /// <param name="modelDef">      The model definition.</param>
        /// <param name="refModelDef">   The reference model definition.</param>
        /// <param name="NamingStrategy">The naming strategy.</param>
        /// <param name="fieldDef">      The field definition.</param>
        /// <returns>The foreign key name.</returns>
        public string GetForeignKeyName(ModelDefinition modelDef, ModelDefinition refModelDef, INamingStrategy NamingStrategy, FieldDefinition fieldDef)
        {
	        if (!String.IsNullOrEmpty(ForeignKeyName)) return ForeignKeyName;
	        var modelName = modelDef.IsInSchema
		        ? modelDef.Schema + "_" + NamingStrategy.GetTableName(modelDef.ModelName)
		        : NamingStrategy.GetTableName(modelDef.ModelName);

	        var refModelName = refModelDef.IsInSchema
		        ? refModelDef.Schema + "_" + NamingStrategy.GetTableName(refModelDef.ModelName)
		        : NamingStrategy.GetTableName(refModelDef.ModelName);
	        return string.Format("FK_{0}_{1}_{2}", modelName, refModelName, fieldDef.FieldName);
        }
    }
}