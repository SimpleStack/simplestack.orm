using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleStack.Orm
{
	public static class PlatformExtensions
	{
		private static readonly Dictionary<string, List<Attribute>> propertyAttributesMap = new Dictionary<string, List<Attribute>>();
		private const string DataContract = "DataContractAttribute";

		private static readonly Regex InvalidVarCharsRegex = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled);
		private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();
		public static object GetDefaultValue(this Type type)
		{
			if (!PlatformExtensions.IsValueType(type))
				return (object)null;
			object instance;
			if (DefaultValueTypes.TryGetValue(type, out instance))
				return instance;
			instance = Activator.CreateInstance(type);
			Dictionary<Type, object> comparand;
			Dictionary<Type, object> dictionary;
			do
			{
				comparand = DefaultValueTypes;
				dictionary = new Dictionary<Type, object>((IDictionary<Type, object>)DefaultValueTypes);
				dictionary[type] = instance;
			}
			while (!object.ReferenceEquals((object)Interlocked.CompareExchange<Dictionary<Type, object>>(ref DefaultValueTypes, dictionary, comparand), (object)comparand));
			return instance;
		}

		public static string SafeVarName(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return (string)null;
			return InvalidVarCharsRegex.Replace(text, "_");
		}

		/// <summary>A ModelDefinition extension method that gets column names.</summary>
		/// <param name="tableType">The tableType to act on.</param>
		/// <returns>The column names.</returns>
		//internal static string GetColumnNames(this Type tableType)
		//{
		//	var modelDefinition = tableType.GetModelDefinition();
		//	return GetColumnNames(modelDefinition);
		//}

		/// <summary>A ModelDefinition extension method that gets column names.</summary>
		/// <param name="modelDef">The modelDef to act on.</param>
		/// <returns>The column names.</returns>
		//public static string GetColumnNames(this ModelDefinition modelDef)
		//{
		//	var sqlColumns = new StringBuilder();
		//	modelDef.FieldDefinitions.ForEach(x =>
		//		sqlColumns.AppendFormat("{0}{1} ", sqlColumns.Length > 0 ? "," : "",
		//		  Config.DialectProvider.GetQuotedColumnName(x.FieldName)));

		//	return sqlColumns.ToString();
		//}

		/// <summary>A string extension method that SQL format.</summary>
		/// <param name="sqlText">  The sqlText to act on.</param>
		/// <param name="sqlParams">Options for controlling the SQL.</param>
		/// <returns>A string.</returns>
		//public static string SqlFormat(this string sqlText, params object[] sqlParams)
		//{
		//	var escapedParams = new List<string>();
		//	foreach (var sqlParam in sqlParams)
		//	{
		//		if (sqlParam == null)
		//		{
		//			escapedParams.Add("NULL");
		//		}
		//		else
		//		{
		//			//var sqlInValues = sqlParam as SqlInValues;
		//			//if (sqlInValues != null)
		//			//{
		//			//	escapedParams.Add(sqlInValues.ToSqlInString());
		//			//}
		//			//else
		//			//{
		//			escapedParams.Add(Config.DialectProvider.GetQuotedValue(sqlParam, sqlParam.GetType()));
		//			//}
		//		}
		//	}
		//	return string.Format(sqlText, escapedParams.ToArray());
		//}



		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (!PlatformExtensions.IsInterface(type))
				return Enumerable.ToArray<PropertyInfo>(Enumerable.Where<PropertyInfo>((IEnumerable<PropertyInfo>)PlatformExtensions.GetTypesPublicProperties(type), (Func<PropertyInfo, bool>)(t => t.GetIndexParameters().Length == 0)));
			List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
			List<Type> list = new List<Type>();
			Queue<Type> queue = new Queue<Type>();
			list.Add(type);
			queue.Enqueue(type);
			while (queue.Count > 0)
			{
				Type type1 = queue.Dequeue();
				foreach (Type type2 in PlatformExtensions.GetTypeInterfaces(type1))
				{
					if (!list.Contains(type2))
					{
						list.Add(type2);
						queue.Enqueue(type2);
					}
				}
				IEnumerable<PropertyInfo> collection = Enumerable.Where<PropertyInfo>((IEnumerable<PropertyInfo>)PlatformExtensions.GetTypesPublicProperties(type1), (Func<PropertyInfo, bool>)(x => !propertyInfos.Contains(x)));
				propertyInfos.InsertRange(0, collection);
			}
			return propertyInfos.ToArray();
		}

		public static bool IsInterface(this Type type)
		{
			return type.IsInterface;
		}

		public static bool IsArray(this Type type)
		{
			return type.IsArray;
		}

		public static bool IsValueType(this Type type)
		{
			return type.IsValueType;
		}

		public static bool IsGeneric(this Type type)
		{
			return type.IsGenericType;
		}

		public static Type BaseType(this Type type)
		{
			return type.BaseType;
		}

		public static Type ReflectedType(this PropertyInfo pi)
		{
			return pi.ReflectedType;
		}

		public static Type ReflectedType(this FieldInfo fi)
		{
			return fi.ReflectedType;
		}

		public static Type GenericTypeDefinition(this Type type)
		{
			return type.GetGenericTypeDefinition();
		}

		public static Type[] GetTypeInterfaces(this Type type)
		{
			return type.GetInterfaces();
		}

		public static Type[] GetTypeGenericArguments(this Type type)
		{
			return type.GetGenericArguments();
		}

		public static ConstructorInfo GetEmptyConstructor(this Type type)
		{
			return type.GetConstructor(Type.EmptyTypes);
		}

		internal static PropertyInfo[] GetTypesPublicProperties(this Type subType)
		{
			return subType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		}

		public static PropertyInfo[] Properties(this Type type)
		{
			return type.GetProperties();
		}

		public static MemberInfo[] GetPublicMembers(this Type type)
		{
			return type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
		}

		public static MemberInfo[] GetAllPublicMembers(this Type type)
		{
			return type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		}

		public static bool HasAttribute<T>(this Type type)
		{
			return Enumerable.Any<object>((IEnumerable<object>)PlatformExtensions.AllAttributes(type), (Func<object, bool>)(x => x.GetType() == typeof(T)));
		}

		public static bool HasAttributeNamed(this Type type, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return Enumerable.Any<object>((IEnumerable<object>)PlatformExtensions.AllAttributes(type), (Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
		}

		public static bool HasAttributeNamed(this PropertyInfo pi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return Enumerable.Any<object>((IEnumerable<object>)PlatformExtensions.AllAttributes(pi), (Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
		}

		public static bool HasAttributeNamed(this FieldInfo fi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return Enumerable.Any<object>((IEnumerable<object>)PlatformExtensions.AllAttributes(fi), (Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
		}

		public static bool HasAttributeNamed(this MemberInfo mi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return Enumerable.Any<object>((IEnumerable<object>)PlatformExtensions.AllAttributes(mi), (Func<object, bool>)(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr));
		}

		public static MethodInfo PropertyGetMethod(this PropertyInfo pi, bool nonPublic = false)
		{
			return pi.GetGetMethod(false);
		}

		public static Type[] Interfaces(this Type type)
		{
			return type.GetInterfaces();
		}

		public static PropertyInfo[] AllProperties(this Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static Type AddAttributes(this Type type, params Attribute[] attrs)
		{
			TypeDescriptor.AddAttributes(type, attrs);
			return type;
		}



		public static object[] AllAttributes(this ParameterInfo paramInfo)
		{
			return paramInfo.GetCustomAttributes(true);
		}

		public static object[] AllAttributes(this FieldInfo fieldInfo)
		{
			return fieldInfo.GetCustomAttributes(true);
		}

		public static object[] AllAttributes(this MemberInfo memberInfo)
		{
			return memberInfo.GetCustomAttributes(true);
		}

		public static object[] AllAttributes(this ParameterInfo paramInfo, Type attrType)
		{
			return paramInfo.GetCustomAttributes(attrType, true);
		}

		public static object[] AllAttributes(this MemberInfo memberInfo, Type attrType)
		{
			return memberInfo.GetCustomAttributes(attrType, true);
		}

		public static object[] AllAttributes(this FieldInfo fieldInfo, Type attrType)
		{
			return fieldInfo.GetCustomAttributes(attrType, true);
		}

		public static object[] AllAttributes(this Type type)
		{
			return Enumerable.ToArray<object>(Enumerable.Cast<object>((IEnumerable)TypeDescriptor.GetAttributes(type)));
		}

		public static object[] AllAttributes(this Type type, Type attrType)
		{
			return (object[])Enumerable.ToArray<Attribute>(Enumerable.OfType<Attribute>((IEnumerable)TypeDescriptor.GetAttributes(type)));
		}

		public static TAttr[] AllAttributes<TAttr>(this ParameterInfo pi)
		{
			return Enumerable.ToArray<TAttr>(Enumerable.Cast<TAttr>((IEnumerable)PlatformExtensions.AllAttributes(pi, typeof(TAttr))));
		}

		public static TAttr[] AllAttributes<TAttr>(this MemberInfo mi)
		{
			return Enumerable.ToArray<TAttr>(Enumerable.Cast<TAttr>((IEnumerable)PlatformExtensions.AllAttributes(mi, typeof(TAttr))));
		}

		public static TAttr[] AllAttributes<TAttr>(this FieldInfo fi)
		{
			return Enumerable.ToArray<TAttr>(Enumerable.Cast<TAttr>((IEnumerable)PlatformExtensions.AllAttributes(fi, typeof(TAttr))));
		}

		public static TAttr[] AllAttributes<TAttr>(this PropertyInfo pi)
		{
			return Enumerable.ToArray<TAttr>(Enumerable.Cast<TAttr>((IEnumerable)PlatformExtensions.AllAttributes(pi, typeof(TAttr))));
		}

		public static TAttr[] AllAttributes<TAttr>(this Type type)
		{
			return Enumerable.ToArray<TAttr>(Enumerable.OfType<TAttr>((IEnumerable)TypeDescriptor.GetAttributes(type)));
		}

		public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : class
		{
			return Enumerable.FirstOrDefault<TAttr>(Enumerable.OfType<TAttr>((IEnumerable)TypeDescriptor.GetAttributes(type)));
		}

		public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo)
		{
			return Enumerable.FirstOrDefault<TAttribute>((IEnumerable<TAttribute>)PlatformExtensions.AllAttributes<TAttribute>(memberInfo));
		}

		public static TAttribute FirstAttribute<TAttribute>(this ParameterInfo paramInfo)
		{
			return Enumerable.FirstOrDefault<TAttribute>((IEnumerable<TAttribute>)PlatformExtensions.AllAttributes<TAttribute>(paramInfo));
		}

		public static TAttribute FirstAttribute<TAttribute>(this PropertyInfo propertyInfo)
		{
			return Enumerable.FirstOrDefault<TAttribute>((IEnumerable<TAttribute>)PlatformExtensions.AllAttributes<TAttribute>(propertyInfo));
		}

		public static bool IsDynamic(this Assembly assembly)
		{
			try
			{
				return assembly is AssemblyBuilder || string.IsNullOrEmpty(assembly.Location);
			}
			catch (NotSupportedException ex)
			{
				return true;
			}
		}

		public static MethodInfo GetPublicStaticMethod(this Type type, string methodName, Type[] types = null)
		{
			if (types != null)
				return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, (Binder)null, types, (ParameterModifier[])null);
			return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
		}

		public static MethodInfo GetMethodInfo(this Type type, string methodName, Type[] types = null)
		{
			if (types != null)
				return type.GetMethod(methodName, types);
			return type.GetMethod(methodName);
		}

		public static object InvokeMethod(this Delegate fn, object instance, object[] parameters = null)
		{
			return fn.Method.Invoke(instance, parameters ?? new object[0]);
		}

		public static FieldInfo GetPublicStaticField(this Type type, string fieldName)
		{
			return type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
		}

		public static Delegate MakeDelegate(this MethodInfo mi, Type delegateType, bool throwOnBindFailure = true)
		{
			return Delegate.CreateDelegate(delegateType, mi, throwOnBindFailure);
		}

		public static Type[] GenericTypeArguments(this Type type)
		{
			return type.GetGenericArguments();
		}

		public static ConstructorInfo[] DeclaredConstructors(this Type type)
		{
			return type.GetConstructors();
		}

		public static bool AssignableFrom(this Type type, Type fromType)
		{
			return type.IsAssignableFrom(fromType);
		}

		public static bool IsStandardClass(this Type type)
		{
			if (type.IsClass && !type.IsAbstract)
				return !type.IsInterface;
			return false;
		}

		public static bool IsAbstract(this Type type)
		{
			return type.IsAbstract;
		}

		public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
		{
			return type.GetProperty(propertyName);
		}

		public static FieldInfo GetFieldInfo(this Type type, string fieldName)
		{
			return type.GetField(fieldName);
		}

		public static FieldInfo[] GetWritableFields(this Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
		}

		public static MethodInfo SetMethod(this PropertyInfo pi, bool nonPublic = true)
		{
			return pi.GetSetMethod(nonPublic);
		}

		public static MethodInfo GetMethodInfo(this PropertyInfo pi, bool nonPublic = true)
		{
			return pi.GetGetMethod(nonPublic);
		}

		public static bool InstanceOfType(this Type type, object instance)
		{
			return type.IsInstanceOfType(instance);
		}

		public static bool IsClass(this Type type)
		{
			return type.IsClass;
		}

		public static bool IsEnum(this Type type)
		{
			return type.IsEnum;
		}

		public static bool IsEnumFlags(this Type type)
		{
			if (type.IsEnum)
				return PlatformExtensions.FirstAttribute<FlagsAttribute>(type) != null;
			return false;
		}

		public static bool IsUnderlyingEnum(this Type type)
		{
			if (!type.IsEnum)
				return type.UnderlyingSystemType.IsEnum;
			return true;
		}

		public static MethodInfo[] GetMethodInfos(this Type type)
		{
			return type.GetMethods();
		}

		public static PropertyInfo[] GetPropertyInfos(this Type type)
		{
			return type.GetProperties();
		}

		public static bool IsGenericTypeDefinition(this Type type)
		{
			return type.IsGenericTypeDefinition;
		}

		public static bool IsGenericType(this Type type)
		{
			return type.IsGenericType;
		}

		public static string GetDeclaringTypeName(this Type type)
		{
			if (type.DeclaringType != (Type)null)
				return type.DeclaringType.Name;
			if (type.ReflectedType != (Type)null)
				return type.ReflectedType.Name;
			return (string)null;
		}

		public static string GetDeclaringTypeName(this MemberInfo mi)
		{
			if (mi.DeclaringType != (Type)null)
				return mi.DeclaringType.Name;
			return mi.ReflectedType.Name;
		}
	}
}