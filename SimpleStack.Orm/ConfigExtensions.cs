using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using SimpleStack.Orm.Attributes;
using RequiredAttribute = SimpleStack.Orm.Attributes.RequiredAttribute;

namespace SimpleStack.Orm
{
	/// <summary>An ORM lite configuration extensions.</summary>
	internal static class OrmLiteConfigExtensions
	{
		/// <summary>The type model definition map.</summary>
		private static Dictionary<Type, ModelDefinition> typeModelDefinitionMap = new Dictionary<Type, ModelDefinition>();

		/// <summary>Query if 'theType' is nullable type.</summary>
		/// <param name="theType">Type of the.</param>
		/// <returns>true if nullable type, false if not.</returns>
		private static bool IsNullableType(Type theType)
		{
			return (theType.IsGenericType
				&& theType.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		/// <summary>Check for identifier field.</summary>
		/// <param name="objProperties">The object properties.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		internal static bool CheckForIdField(IEnumerable<PropertyInfo> objProperties)
		{
			// Not using Linq.Where() and manually iterating through objProperties just to avoid dependencies on System.Xml??
			foreach (var objProperty in objProperties)
			{
				if (objProperty.Name != Config.IdField) continue;
				return true;
			}
			return false;
		}

		/// <summary>Clears the cache.</summary>
		internal static void ClearCache()
		{
			typeModelDefinitionMap = new Dictionary<Type, ModelDefinition>();
		}

		/// <summary>A Type extension method that initialises this object.</summary>
		/// <param name="modelType">The modelType to act on.</param>
		/// <returns>A ModelDefinition.</returns>
		public static ModelDefinition Init(this Type modelType)
		{
			return modelType.GetModelDefinition();
		}

		/// <summary>A Type extension method that gets model definition.</summary>
		/// <param name="modelType">The modelType to act on.</param>
		/// <returns>The model definition.</returns>
		internal static ModelDefinition GetModelDefinition(this Type modelType)
		{
			ModelDefinition modelDef;

			if (typeModelDefinitionMap.TryGetValue(modelType, out modelDef))
				return modelDef;

			if (modelType.IsValueType() || modelType == typeof(string))
				return null;

			var modelAliasAttr = modelType.FirstAttribute<AliasAttribute>();
			var schemaAttr = modelType.FirstAttribute<SchemaAttribute>();
			modelDef = new ModelDefinition
			{
				ModelType = modelType,
				Name = modelType.Name,
				Alias = modelAliasAttr != null ? modelAliasAttr.Name : null,
				Schema = schemaAttr != null ? schemaAttr.Name : null
			};

			modelDef.CompositeIndexes.AddRange(
				modelType.GetCustomAttributes(typeof(CompositeIndexAttribute), true).ToList()
				.ConvertAll(x => (CompositeIndexAttribute)x));

			var objProperties = modelType.GetProperties(
				BindingFlags.Public | BindingFlags.Instance).ToList();

			var hasIdField = CheckForIdField(objProperties);

			var i = 0;
			foreach (var propertyInfo in objProperties)
			{
				var sequenceAttr = propertyInfo.FirstAttribute<SequenceAttribute>();
				var computeAttr = propertyInfo.FirstAttribute<ComputeAttribute>();
				var pkAttribute = propertyInfo.FirstAttribute<PrimaryKeyAttribute>();
				var decimalAttribute = propertyInfo.FirstAttribute<DecimalLengthAttribute>();
				var belongToAttribute = propertyInfo.FirstAttribute<BelongToAttribute>();
				var isFirst = i++ == 0;

				var isPrimaryKey = propertyInfo.Name == Config.IdField /* || (!hasIdField && isFirst)*/ || pkAttribute != null;

				var isNullableType = IsNullableType(propertyInfo.PropertyType);

				var isNullable = (!propertyInfo.PropertyType.IsValueType
								   && propertyInfo.FirstAttribute<RequiredAttribute>() == null)
								 || isNullableType;

				var propertyType = isNullableType
					? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
					: propertyInfo.PropertyType;

				var aliasAttr = propertyInfo.FirstAttribute<AliasAttribute>();

				var indexAttr = propertyInfo.FirstAttribute<IndexAttribute>();
				var isIndex = indexAttr != null;
				var isUnique = isIndex && indexAttr.Unique;

				var stringLengthAttr = propertyInfo.FirstAttribute<StringLengthAttribute>();

				var defaultValueAttr = propertyInfo.FirstAttribute<DefaultAttribute>();

				var referencesAttr = propertyInfo.FirstAttribute<ReferencesAttribute>();
				var foreignKeyAttr = propertyInfo.FirstAttribute<ForeignKeyAttribute>();

				if (decimalAttribute != null && stringLengthAttr == null)
					stringLengthAttr = new StringLengthAttribute(decimalAttribute.Precision);

				var fieldDefinition = new FieldDefinition
				{
					Name = propertyInfo.Name,
					Alias = aliasAttr != null ? aliasAttr.Name : null,
					FieldType = propertyType,
					PropertyInfo = propertyInfo,
					IsNullable = isNullable,
					IsPrimaryKey = isPrimaryKey,
					AutoIncrement =
						isPrimaryKey &&
						propertyInfo.FirstAttribute<AutoIncrementAttribute>() != null,
					IsIndexed = isIndex,
					IsUnique = isUnique,
					FieldLength =
						stringLengthAttr != null
							? stringLengthAttr.MaximumLength
							: (int?)null,
					DefaultValue =
						defaultValueAttr != null ? defaultValueAttr.DefaultValue : null,
					ForeignKey =
						foreignKeyAttr == null
							? referencesAttr == null
								  ? null
								  : new ForeignKeyConstraint(referencesAttr.Type)
							: new ForeignKeyConstraint(foreignKeyAttr.Type,
													   foreignKeyAttr.OnDelete,
													   foreignKeyAttr.OnUpdate,
													   foreignKeyAttr.ForeignKeyName),
					GetValueFn = propertyInfo.GetPropertyGetterFn(),
					SetValueFn = propertyInfo.GetPropertySetterFn(),
					Sequence = sequenceAttr != null ? sequenceAttr.Name : string.Empty,
					IsComputed = computeAttr != null,
					ComputeExpression =
						computeAttr != null ? computeAttr.Expression : string.Empty,
					Scale = decimalAttribute != null ? decimalAttribute.Scale : (int?)null,
					BelongToModelName = belongToAttribute != null ? belongToAttribute.BelongToTableType.GetModelDefinition().ModelName : null,
				};

				if (propertyInfo.FirstAttribute<IgnoreAttribute>() != null)
					modelDef.IgnoredFieldDefinitions.Add(fieldDefinition);
				else
					modelDef.FieldDefinitions.Add(fieldDefinition);
			}

			//modelDef.SqlSelectAllFromTable = String.Format("SELECT {0} FROM {1} ",
			//	Config.DialectProvider.GetColumnNames(modelDef),
			//	Config.DialectProvider.GetQuotedTableName(modelDef));

			Dictionary<Type, ModelDefinition> snapshot, newCache;
			do
			{
				snapshot = typeModelDefinitionMap;
				newCache = new Dictionary<Type, ModelDefinition>(typeModelDefinitionMap);
				newCache[modelType] = modelDef;

			} while (!ReferenceEquals(
				Interlocked.CompareExchange(ref typeModelDefinitionMap, newCache, snapshot), snapshot));

			return modelDef;
		}

	}
}