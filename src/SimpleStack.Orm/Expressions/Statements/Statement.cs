using System;
using Dapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleStack.Orm.Expressions.Statements
{
    public class StatementParameters : KeyedCollection<string, StatementParameter>
    {
        protected override string GetKeyForItem(StatementParameter item)
        {
            return item.ParameterName;
        }
        
        public DynamicParameters ToDynamicParameters()
        {
            var result = new DynamicParameters();
            foreach (var pair in this)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                result.Add(pair.ParameterName, pair.Value,
                    SqlMapper.LookupDbType(pair.Type, pair.ParameterName, demand: true, out _));
#pragma warning restore CS0618 // Type or member is obsolete
            }

            return result;
        }
    }

    public class StatementParameter
    {
        public string ParameterName { get; }
        public Type Type { get; }
        public object Value { get; set; }

        public StatementParameter(string parameterName, Type type, object value)
        {
            ParameterName = parameterName;
            Type = type;
            Value = value;
        }
    }
    
    public abstract class Statement
    {
        public string TableName { get; set; }
        public StatementParameters Parameters { get; } = new StatementParameters();
    }
}