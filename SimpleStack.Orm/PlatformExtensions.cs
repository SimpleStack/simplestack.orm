using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace SimpleStack.Orm
{
	public static class PlatformExtensions
	{
		private static readonly Regex InvalidVarCharsRegex = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled);

		public static string SafeVarName(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return null;
			return InvalidVarCharsRegex.Replace(text, "_");
		}

		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (!IsInterface(type))
				return GetTypesPublicProperties(type).Where(t => t.GetIndexParameters().Length == 0).ToArray();
			var propertyInfos = new List<PropertyInfo>();
			List<Type> list = new List<Type>();
			Queue<Type> queue = new Queue<Type>();
			list.Add(type);
			queue.Enqueue(type);
			while (queue.Count > 0)
			{
				Type type1 = queue.Dequeue();
				foreach (Type type2 in GetTypeInterfaces(type1))
				{
					if (!list.Contains(type2))
					{
						list.Add(type2);
						queue.Enqueue(type2);
					}
				}
				IEnumerable<PropertyInfo> collection = GetTypesPublicProperties(type1).Where(x => !propertyInfos.Contains(x));
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
			return AllAttributes(type).Any(x => x.GetType() == typeof(T));
		}

		public static bool HasAttributeNamed(this Type type, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return AllAttributes(type).Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
		}

		public static bool HasAttributeNamed(this PropertyInfo pi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return AllAttributes(pi).Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
		}

		public static bool HasAttributeNamed(this FieldInfo fi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return AllAttributes(fi).Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
		}

		public static bool HasAttributeNamed(this MemberInfo mi, string name)
		{
			string normalizedAttr = name.Replace("Attribute", "").ToLower();
			return AllAttributes(mi).Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
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

		public static Attribute[] AllAttributes(this Type type)
		{
			return TypeDescriptor.GetAttributes(type).Cast<Attribute>().ToArray();
		}

		public static Attribute[] AllAttributes(this Type type, Type attrType)
		{
			return TypeDescriptor.GetAttributes(type).OfType<Attribute>().ToArray();
		}

		public static TAttr[] AllAttributes<TAttr>(this ParameterInfo pi)
		{
			return AllAttributes(pi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
		}

		public static TAttr[] AllAttributes<TAttr>(this MemberInfo mi)
		{
			return AllAttributes(mi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
		}

		public static TAttr[] AllAttributes<TAttr>(this FieldInfo fi)
		{
			return AllAttributes(fi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
		}

		public static TAttr[] AllAttributes<TAttr>(this PropertyInfo pi)
		{
			return AllAttributes(pi, typeof(TAttr)).Cast<TAttr>().ToArray<TAttr>();
		}

		public static TAttr[] AllAttributes<TAttr>(this Type type)
		{
			return TypeDescriptor.GetAttributes(type).OfType<TAttr>().ToArray<TAttr>();
		}

		public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : class
		{
			return TypeDescriptor.GetAttributes(type).OfType<TAttr>().FirstOrDefault<TAttr>();
		}

		public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo)
		{
			return AllAttributes<TAttribute>(memberInfo).FirstOrDefault<TAttribute>();
		}

		public static TAttribute FirstAttribute<TAttribute>(this ParameterInfo paramInfo)
		{
			return AllAttributes<TAttribute>(paramInfo).FirstOrDefault<TAttribute>();
		}

		public static TAttribute FirstAttribute<TAttribute>(this PropertyInfo propertyInfo)
		{
			return AllAttributes<TAttribute>(propertyInfo).FirstOrDefault<TAttribute>();
		}

		public static bool IsDynamic(this Assembly assembly)
		{
			try
			{
				return assembly is AssemblyBuilder || string.IsNullOrEmpty(assembly.Location);
			}
			catch (NotSupportedException)
			{
				return true;
			}
		}

		public static MethodInfo GetPublicStaticMethod(this Type type, string methodName, Type[] types = null)
		{
			if (types != null)
				return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, types, null);
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
				return FirstAttribute<FlagsAttribute>(type) != null;
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
			return type.DeclaringType?.Name ?? type.ReflectedType?.Name;
		}

		public static string GetDeclaringTypeName(this MemberInfo mi)
		{
			return mi.DeclaringType?.Name ?? mi.ReflectedType?.Name;
		}
	}
}