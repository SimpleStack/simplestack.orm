using System;

namespace SimpleStack.Orm
{
    /// <summary>A foreign key constraint.</summary>
    public class ForeignKeyConstraint
    {
        /// <summary>
        ///     Initializes a new instance of the NServiceKit.OrmLite.ForeignKeyConstraint class.
        /// </summary>
        /// <param name="type">          The type.</param>
        /// <param name="onDelete">      The on delete.</param>
        /// <param name="onUpdate">      The on update.</param>
        /// <param name="foreignKeyName">The name of the foreign key.</param>
        public ForeignKeyConstraint(Type type, string onDelete = null, string onUpdate = null,
            string foreignKeyName = null)
        {
            ReferenceType = type;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
            ForeignKeyName = foreignKeyName;
        }

        /// <summary>Gets the type of the reference.</summary>
        /// <value>The type of the reference.</value>
        public Type ReferenceType { get; }

        /// <summary>Gets the on delete.</summary>
        /// <value>The on delete.</value>
        public string OnDelete { get; }

        /// <summary>Gets the on update.</summary>
        /// <value>The on update.</value>
        public string OnUpdate { get; }

        /// <summary>Gets the name of the foreign key.</summary>
        /// <value>The name of the foreign key.</value>
        public string ForeignKeyName { get; }

        /// <summary>Gets foreign key name.</summary>
        /// <param name="modelDef">      The model definition.</param>
        /// <param name="refModelDef">   The reference model definition.</param>
        /// <param name="namingStrategy">The naming strategy.</param>
        /// <param name="fieldDef">      The field definition.</param>
        /// <returns>The foreign key name.</returns>
        public string GetForeignKeyName(ModelDefinition modelDef, ModelDefinition refModelDef,
            INamingStrategy namingStrategy, FieldDefinition fieldDef)
        {
            if (!string.IsNullOrEmpty(ForeignKeyName))
            {
                return ForeignKeyName;
            }

            var modelName = modelDef.IsInSchema
                ? modelDef.Schema + "_" + namingStrategy.GetTableName(modelDef.ModelName)
                : namingStrategy.GetTableName(modelDef.ModelName);

            var refModelName = refModelDef.IsInSchema
                ? refModelDef.Schema + "_" + namingStrategy.GetTableName(refModelDef.ModelName)
                : namingStrategy.GetTableName(refModelDef.ModelName);
            return $"FK_{modelName}_{refModelName}_{fieldDef.FieldName}";
        }
    }
}