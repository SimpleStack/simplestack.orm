using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm
{
    /// <summary>An ORM lite configuration extensions.</summary>
    internal static class OrmLiteConfigExtensions
    {
        /// <summary>The type model definition map.</summary>
        private static Dictionary<Type, ModelDefinition> _typeModelDefinitionMap =
            new Dictionary<Type, ModelDefinition>();

        /// <summary>Query if 'theType' is nullable type.</summary>
        /// <param name="theType">Type of the.</param>
        /// <returns>true if nullable type, false if not.</returns>
        private static bool IsNullableType(Type theType)
        {
            return theType.IsGenericType()
                   && theType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>A Type extension method that gets model definition.</summary>
        /// <param name="modelType">The modelType to act on.</param>
        /// <returns>The model definition.</returns>
        internal static ModelDefinition GetModelDefinition(this Type modelType)
        {
            if (_typeModelDefinitionMap.TryGetValue(modelType, out var modelDef))
            {
                return modelDef;
            }

            if (modelType.IsValueType() || modelType == typeof(string))
            {
                return null;
            }

            var modelAliasAttr = modelType.FirstAttribute<AliasAttribute>();
            var schemaAttr = modelType.FirstAttribute<SchemaAttribute>();
            modelDef = new ModelDefinition
            {
                ModelType = modelType,
                Name = modelType.Name,
                Alias = modelAliasAttr?.Name,
                Schema = schemaAttr?.Name
            };

            modelDef.CompositeIndexes.AddRange(modelType.AlltAttributes<CompositeIndexAttribute>());

            var objProperties = modelType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance).ToList();
            
            foreach (var propertyInfo in objProperties)
            {
                var sequenceAttr = propertyInfo.FirstAttribute<SequenceAttribute>();
                var computeAttr = propertyInfo.FirstAttribute<ComputeAttribute>();
                var pkAttribute = propertyInfo.FirstAttribute<PrimaryKeyAttribute>();
                var decimalAttribute = propertyInfo.FirstAttribute<DecimalLengthAttribute>();

                var isPrimaryKey = pkAttribute != null;

                var isNullableType = IsNullableType(propertyInfo.PropertyType);

                var isNullable = !propertyInfo.PropertyType.IsValueType()
                                 && propertyInfo.FirstAttribute<RequiredAttribute>() == null
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
                {
                    stringLengthAttr = new StringLengthAttribute(decimalAttribute.Precision);
                }

                var fieldDefinition = new FieldDefinition
                {
                    Name = propertyInfo.Name,
                    Alias = aliasAttr != null ? aliasAttr.Name : null,
                    FieldType = propertyType,
                    PropertyInfo = propertyInfo,
                    IsNullable = isNullable,
                    IsPrimaryKey = isPrimaryKey,
                    AutoIncrement = isPrimaryKey && propertyInfo.FirstAttribute<AutoIncrementAttribute>() != null,
                    IsIndexed = isIndex,
                    IsUnique = isUnique,
                    FieldLength = stringLengthAttr?.MaximumLength,
                    DefaultValue = defaultValueAttr?.DefaultValue,
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
                    Sequence = sequenceAttr != null ? sequenceAttr.Name : string.Empty,
                    IsComputed = computeAttr != null,
                    ComputeExpression = computeAttr != null ? computeAttr.Expression : string.Empty,
                    Scale = decimalAttribute?.Scale
                };

                if (propertyInfo.FirstAttribute<IgnoreAttribute>() != null)
                {
                    modelDef.IgnoredFieldDefinitions.Add(fieldDefinition);
                }
                else
                {
                    modelDef.FieldDefinitions.Add(fieldDefinition);
                }
            }

            Dictionary<Type, ModelDefinition> snapshot, newCache;
            do
            {
                snapshot = _typeModelDefinitionMap;
                newCache = new Dictionary<Type, ModelDefinition>(_typeModelDefinitionMap);
                newCache[modelType] = modelDef;
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref _typeModelDefinitionMap, newCache, snapshot), snapshot));

            return modelDef;
        }
    }
}