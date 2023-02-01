using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions.Statements.Typed
{
    public class TypedInsertStatement<T>
    {
        private readonly IDialectProvider _dialectProvider;
        private readonly ModelDefinition _modelDefinition = typeof(T).GetModelDefinition();

        public TypedInsertStatement(IDialectProvider dialectProvider)
        {
            _dialectProvider = dialectProvider;
            Statement.TableName = _dialectProvider.GetQuotedTableName(_modelDefinition);
            Statement.HasIdentity = _modelDefinition.FieldDefinitions.Any(x => x.AutoIncrement);
        }

        public InsertStatement Statement { get; } = new InsertStatement();

        /// <summary>
        ///     Override name of the table to insert values into.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TypedInsertStatement<T> Into(string tableName, string schema = null)
        {
            Statement.TableName = _dialectProvider.GetQuotedTableName(tableName, schema);
            return this;
        }

        public TypedInsertStatement<T> Values<TKey>(T values, Expression<Func<T, TKey>> onlyFields)
        {
            var tfev = new TableWFieldsExpresionVisitor<T>(_dialectProvider,
                Statement.Parameters,
                _modelDefinition,
                false);
            return Values(values, tfev.VisitExpression(onlyFields).Split(','));
        }

        public TypedInsertStatement<T> Values(T values, IEnumerable<string> onlyFields)
        {
            var fieldsArray = onlyFields as string[] ?? onlyFields.ToArray();

            var fields = _modelDefinition.FieldDefinitions
                .Where(fieldDef => !fieldDef.IsComputed)
                .Where(fieldDef => !fieldDef.AutoIncrement)
                .Where(fieldDef => !fieldsArray.Any() || fieldsArray.Contains(fieldDef.Name)).ToList();

            foreach (var fieldDef in fields)
            {
                var v = fieldDef.GetValue(values) ?? fieldDef.DefaultValue;
                if (v != null)
                {
                    var pname = _dialectProvider.GetParameterName(Statement.Parameters.Count);
                    Statement.Parameters.Add(pname, v);
                    Statement.ParameterDefinitions.Add(pname, fieldDef);
                    Statement.InsertFields.Add(_dialectProvider.GetQuotedColumnName(fieldDef.FieldName));
                }
            }

            return this;
        }
    }
}