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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm
{
    /// <summary>A model definition.</summary>
	public class ModelDefinition
	{
        /// <summary>
        /// Initializes a new instance of the NServiceKit.OrmLite.ModelDefinition class.
        /// </summary>
		public ModelDefinition()
		{
			FieldDefinitions = new List<FieldDefinition>();
			IgnoredFieldDefinitions = new List<FieldDefinition>();
			CompositeIndexes = new List<CompositeIndexAttribute>();
		}

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
		public string Name { get; set; }

        /// <summary>Gets or sets the alias.</summary>
        /// <value>The alias.</value>
		public string Alias { get; set; }

        /// <summary>Gets or sets the schema.</summary>
        /// <value>The schema.</value>
        public string Schema { get; set; }

        /// <summary>Gets a value indicating whether this object is in schema.</summary>
        /// <value>true if this object is in schema, false if not.</value>
        public bool IsInSchema => !string.IsNullOrEmpty(Schema);

        /// <summary>Gets the name of the model.</summary>
        /// <value>The name of the model.</value>
		public string ModelName
		{
			get { return this.Alias ?? this.Name; }
		}

        /// <summary>Gets or sets the type of the model.</summary>
        /// <value>The type of the model.</value>
		public Type ModelType { get; set; }

        /// <summary>Gets or sets the SQL select all from table.</summary>
        /// <value>The SQL select all from table.</value>
        //public string SqlSelectAllFromTable { get; set; }

        /// <summary>Gets the primary key.</summary>
        /// <value>The primary key.</value>
		public FieldDefinition PrimaryKey
		{
			get
			{
				return this.FieldDefinitions.First(x => x.IsPrimaryKey);
			}
		}

        /// <summary>Gets or sets the field definitions.</summary>
        /// <value>The field definitions.</value>
		public List<FieldDefinition> FieldDefinitions { get; set; }

        /// <summary>Array of field definitions.</summary>
		private FieldDefinition[] _fieldDefinitionsArray;

        /// <summary>Gets an array of field definitions.</summary>
        /// <value>An Array of field definitions.</value>
		public FieldDefinition[] FieldDefinitionsArray
		{
			get
			{
				if (_fieldDefinitionsArray == null)
				{
					_fieldDefinitionsArray = FieldDefinitions.ToArray();
				}
				return _fieldDefinitionsArray;
			}
		}

        /// <summary>Gets or sets the ignored field definitions.</summary>
        /// <value>The ignored field definitions.</value>
		public List<FieldDefinition> IgnoredFieldDefinitions { get; set; }

        /// <summary>Array of ignored field definitions.</summary>
		private FieldDefinition[] ignoredFieldDefinitionsArray;

        /// <summary>Gets an array of ignored field definitions.</summary>
        /// <value>An Array of ignored field definitions.</value>
		public FieldDefinition[] IgnoredFieldDefinitionsArray
		{
			get
			{
				if (ignoredFieldDefinitionsArray == null)
				{
					ignoredFieldDefinitionsArray = IgnoredFieldDefinitions.ToArray();
				}
				return ignoredFieldDefinitionsArray;
			}
		}

        /// <summary>Array of all field definitions.</summary>
		private FieldDefinition[] allFieldDefinitionsArray;

        /// <summary>Gets an array of all field definitions.</summary>
        /// <value>An Array of all field definitions.</value>
		public FieldDefinition[] AllFieldDefinitionsArray
		{
			get
			{
				if (allFieldDefinitionsArray == null)
				{
					List<FieldDefinition> allItems = new List<FieldDefinition>(FieldDefinitions);
					allItems.AddRange(IgnoredFieldDefinitions);
					allFieldDefinitionsArray = allItems.ToArray(); 
				}
				return allFieldDefinitionsArray;
			}
		}

        /// <summary>Gets or sets the composite indexes.</summary>
        /// <value>The composite indexes.</value>
		public List<CompositeIndexAttribute> CompositeIndexes { get; set; }

        /// <summary>Gets field definition.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>The field definition.</returns>
		public FieldDefinition GetFieldDefinition<T>(Expression<Func<T,object>> field)
		{
			var fn = GetFieldName (field);
			return  FieldDefinitions.First(f=>f.Name==fn );
		}

        /// <summary>Gets field name.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="field">The field.</param>
        /// <returns>The field name.</returns>
		string GetFieldName<T>(Expression<Func<T,object>> field){
			
			var lambda = (field as LambdaExpression);
			if( lambda.Body.NodeType==ExpressionType.MemberAccess)
			{
				var me = lambda.Body as MemberExpression;
				return me.Member.Name;
			}
			else
			{
				var operand = (lambda.Body as UnaryExpression).Operand ;
				return (operand as MemberExpression).Member.Name;
			}
		}

	}

    /// <summary>A model definition.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    public static class ModelDefinition<T>
    {
        /// <summary>The definition.</summary>
        private static ModelDefinition definition;

        /// <summary>Gets the definition.</summary>
        /// <value>The definition.</value>
		public static ModelDefinition Definition
		{
			get { return definition ?? (definition = typeof(T).GetModelDefinition()); }
		}

		///// <summary>Name of the primary key.</summary>
		//private static string primaryKeyName;

		///// <summary>Gets the name of the primary key.</summary>
		///// <value>The name of the primary key.</value>
		//public static string PrimaryKeyName
		//{
		//	get { return primaryKeyName ?? (primaryKeyName = Definition.PrimaryKey.FieldName); }
		//}

	}
}