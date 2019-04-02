using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStack.Orm.Expressions
{
	/// <summary>A sql.</summary>
	public static class Sql
	{
		/// <summary>Insert.</summary>
		/// <typeparam name="T">    Generic type parameter.</typeparam>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="list"> The list.</param>
		/// <returns>true if it succeeds, false if it fails.</returns>
		public static bool In<T, TItem>(T value, params TItem[] list)
		{
			return value != null && Flatten(list).Any(obj => obj.ToString() == value.ToString());
		}

		/// <summary>Flattens the given list.</summary>
		/// <param name="list">The list.</param>
		/// <returns>A List&lt;object&gt;</returns>
		public static List<object> Flatten(IEnumerable list)
		{
			var ret = new List<object>();
			if (list == null) return ret;

			foreach (var item in list)
			{
				if (item == null) continue;

				var arr = item as IEnumerable;
				if (arr != null && !(item is string))
				{
					ret.AddRange(arr.Cast<object>());
				}
				else
				{
					ret.Add(item);
				}
			}
			return ret;
		}

		/// <summary>Descriptions the given value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>A string.</returns>
		public static string Desc<T>(T value)
		{
			return value == null ? string.Empty : value + " DESC";
		}

		/// <summary>As.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">  The value.</param>
		/// <param name="asValue">as value.</param>
		/// <returns>A string.</returns>
		public static string As<T>(T value, object asValue)
		{
			return value == null ? string.Empty : $"{value} AS {asValue}";
		}

		/// <summary>Sums the given value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>A T.</returns>
		public static T Sum<T>(T value)
		{
			return value;
		}

		/// <summary>Counts the given value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>A T.</returns>
		public static T Count<T>(T value)
		{
			return value;
		}

		/// <summary>Determines the minimum of the given parameters.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>The minimum value.</returns>
		public static T Min<T>(T value)
		{
			return value;
		}

		/// <summary>Determines the maximum of the given parameters.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>The maximum value.</returns>
		public static T Max<T>(T value)
		{
			return value;
		}

		/// <summary>Determines the average of the given parameters.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>The average value.</returns>
		public static T Avg<T>(T value)
		{
			return value;
		}
        [Obsolete("Use DateTime property instead")]
		public static int Year(DateTime value)
		{
			return value.Year;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Month(DateTime value)
		{
			return value.Month;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Day(DateTime value)
		{
			return value.Day;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Hour(DateTime value)
		{
			return value.Hour;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Minute(DateTime value)
		{
			return value.Minute;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Second(DateTime value)
		{
			return value.Second;
		}
	    [Obsolete("Use DateTime property instead")]
        public static int Quarter(DateTime value)
		{
			return ((value.Month - 1) / 3) + 1;
		}
	}
}

