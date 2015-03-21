using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Dapper;
using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm
{
	public class EnumAliasTypeHandler : SqlMapper.ITypeHandler
	{
		private readonly Dictionary<Type, Dictionary<string, object>> _enumFromAlias = new Dictionary<Type, Dictionary<string, object>>();
		private readonly Dictionary<Type, Dictionary<object, string>> _aliasFromEnum = new Dictionary<Type, Dictionary<object, string>>();

		public void SetValue(IDbDataParameter parameter, object value)
		{
			parameter.DbType = DbType.String;
			if (value == null)
			{
				parameter.Value = DBNull.Value;
			}
			else
			{
				parameter.Value = GetAliasFromEnum(value);
			}
		}

		public object Parse(Type destinationType, object value)
		{
			return GetEnumFromAlias(destinationType, value.ToString());
		}

		private object GetEnumFromAlias(Type destinationType, string alias)
		{
			if (!_enumFromAlias.ContainsKey(destinationType))
			{
				LoadEnumType(destinationType);
			}
			return _enumFromAlias[destinationType][alias.ToUpper()];
		}
		private string GetAliasFromEnum(object value)
		{
			var type = value.GetType();
			if (!_enumFromAlias.ContainsKey(type))
			{
				LoadEnumType(type);
			}
			return _aliasFromEnum[type][value];
		}

		private void LoadEnumType(Type type)
		{
			if(_aliasFromEnum.ContainsKey(type))
				return;

			if(!type.IsEnum)
				throw  new Exception("Invalid type, must be an enum");

			var aliasFromEnums = new Dictionary<object, string>();
			var enumFromAliases =  new Dictionary<string, object>();

			foreach (var enumValue in Enum.GetValues(type))
			{
				var memInfo = type.GetMember(enumValue.ToString());
				var attributes = memInfo[0].GetCustomAttributes(typeof(AliasAttribute),
					false);
				var alias = ((AliasAttribute)attributes[0]).Name;

				aliasFromEnums.Add(enumValue,alias);
				enumFromAliases.Add(alias,enumValue);
			}

			_aliasFromEnum.Add(type,aliasFromEnums);
			_enumFromAlias.Add(type,enumFromAliases);
		}
	}
}