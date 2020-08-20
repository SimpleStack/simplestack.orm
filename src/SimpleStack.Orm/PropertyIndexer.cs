using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleStack.Orm
{
    /// <summary>Property getter delegate.</summary>
    /// <param name="instance">The instance.</param>
    /// <returns>An object.</returns>
    public delegate object PropertyGetterDelegate(object instance);

    /// <summary>A property invoker.</summary>
    public static class PropertyInvoker
    {
        /// <summary>A PropertyInfo extension method that gets property getter function.</summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="propertyInfo">The propertyInfo to act on.</param>
        /// <returns>The property getter function.</returns>
        public static PropertyGetterDelegate GetPropertyGetterFn(this PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod();
            if (getMethodInfo == null)
            {
                return null;
            }

#if NO_EXPRESSIONS
			return o => propertyInfo.GetGetMethod().Invoke(o, new object[] { });
#else
            var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
            var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.DeclaringType);

            var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
            var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

            var propertyGetFn = Expression.Lambda<PropertyGetterDelegate>
            (
                oExprCallPropertyGetFn,
                oInstanceParam
            ).Compile();

            return propertyGetFn;
#endif
        }
    }
}