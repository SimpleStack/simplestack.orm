using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SimpleStack.Orm
{
    public static class PlatformExtensions
    {
        private static readonly Regex InvalidVarCharsRegex = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled);

        public static string SafeVarName(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return InvalidVarCharsRegex.Replace(text, "_");
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (!IsInterface(type))
            {
                return GetTypesPublicProperties(type).Where(t => t.GetIndexParameters().Length == 0).ToArray();
            }

            var propertyInfos = new List<PropertyInfo>();
            var list = new List<Type>();
            var queue = new Queue<Type>();
            list.Add(type);
            queue.Enqueue(type);
            while (queue.Count > 0)
            {
                var type1 = queue.Dequeue();
                foreach (var type2 in GetTypeInterfaces(type1))
                {
                    if (!list.Contains(type2))
                    {
                        list.Add(type2);
                        queue.Enqueue(type2);
                    }
                }

                var collection = GetTypesPublicProperties(type1).Where(x => !propertyInfos.Contains(x));
                propertyInfos.InsertRange(0, collection);
            }

            return propertyInfos.ToArray();
        }

        public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : Attribute
        {
#if NET45
			return type.GetCustomAttributes<TAttr>().FirstOrDefault();
#else
            return type.GetTypeInfo().GetCustomAttributes<TAttr>().FirstOrDefault();
#endif
        }

        public static TAttr FirstAttribute<TAttr>(this MemberInfo mi) where TAttr : Attribute
        {
#if NET45
			return mi.GetCustomAttributes<TAttr>().FirstOrDefault();
#else
            return mi.GetCustomAttributes<TAttr>().FirstOrDefault();
#endif
        }

        public static IEnumerable<TAttr> AlltAttributes<TAttr>(this Type type) where TAttr : Attribute
        {
#if NET45
			return type.GetCustomAttributes<TAttr>();
#else
            return type.GetTypeInfo().GetCustomAttributes<TAttr>();
#endif
        }

        public static TAttr FirstAttribute<TAttr>(this PropertyInfo pi) where TAttr : Attribute
        {
            return pi.GetCustomAttributes<TAttr>().FirstOrDefault();
        }

        public static IEnumerable<TAttr> AlltAttributes<TAttr>(this PropertyInfo pi) where TAttr : Attribute
        {
            return pi.GetCustomAttributes<TAttr>();
        }

        public static bool IsInterface(this Type type)
        {
#if NET45
			return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

        public static bool IsArray(this Type type)
        {
            return type.IsArray;
        }

        public static bool IsValueType(this Type type)
        {
#if NET45
			return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        public static bool IsGeneric(this Type type)
        {
#if NET45
			return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if NET45
			return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
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

        public static FieldInfo GetPublicStaticField(this Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
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

        public static bool IsAbstract(this Type type)
        {
#if NET45
			return type.IsAbstract;
#else
            return type.GetTypeInfo().IsAbstract;
#endif
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName);
        }

        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            return type.GetField(fieldName);
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
#if NET45
			return type.IsClass;
#else
            return type.GetTypeInfo().IsClass;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NET45
			return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
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
#if NET45
			return type.UnderlyingSystemType.IsGenericTypeDefinition;
#else
            return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NET45
			return type.UnderlyingSystemType.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool EqualsIgnoreCase(this string val, string val2)
        {
#if NET45
			return string.Equals(val, val2, StringComparison.InvariantCultureIgnoreCase);
#else
            return string.Equals(val, val2, StringComparison.OrdinalIgnoreCase);
#endif
        }

        public static MemberInfo[] GetMemberByName(this Type type, string memberName)
        {
#if NET45
			return type.GetMember(memberName);
#else
            return type.GetTypeInfo().GetMember(memberName);
#endif
        }

        public static PropertyInfo[] GetProperties(this Type type)
        {
#if NET45
			return type.GetProperties();
#else
            return type.GetTypeInfo().GetProperties();
#endif
        }
    }
}