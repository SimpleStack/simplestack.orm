using Dapper;
using System.Collections.Generic;

namespace SimpleStack.Orm.Expressions.Statements
{
    public abstract class Statement
    {
        public string TableName { get; set; }
        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
        public IDictionary<string, FieldDefinition> ParameterDefinitions { get; } = new Dictionary<string, FieldDefinition>();
    }

    public static class StatementExtensions
    {
        // This is to fix MSSQL provider error happen on UpdateStatement during update binary column with null value:
        //   "Implicit conversion from data type nvarchar to varbinary is not allowed. Use the CONVERT function to run this query."
        // When null parameter passed as object Dapper do not know the type and by default type is set to DbType.String.
        public static DynamicParameters GetDynamicParameters(this Statement statement)
        {
            var result = new DynamicParameters();
            foreach (var pair in statement.Parameters)
            {
                if (statement.ParameterDefinitions.TryGetValue(pair.Key, out var definition))
#pragma warning disable CS0618 // Type or member is obsolete
                    result.Add(pair.Key, pair.Value, SqlMapper.LookupDbType(definition.FieldType, pair.Key, demand: true, out _));
#pragma warning restore CS0618 // Type or member is obsolete
                else
                    result.Add(pair.Key, pair.Value);
            }
            return result;
        }
    }
}