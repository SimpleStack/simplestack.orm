//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace SimpleStack.Orm
//{
//	/// <summary>
//	/// Creates an instance of a Type from a string value
//	/// 
//	/// </summary>
//	public static class TypeSerializer
//	{
//		private static readonly UTF8Encoding UTF8EncodingWithoutBom = new UTF8Encoding(false);
//		public const string DoubleQuoteString = "\"\"";

//		/// <summary>
//		/// Determines whether the specified type is convertible from string.
//		/// 
//		/// </summary>
//		/// <param name="type">The type.</param>
//		/// <returns>
//		/// <c>true</c> if the specified type is convertible from string; otherwise, <c>false</c>.
//		/// 
//		/// </returns>
//		public static bool CanCreateFromString(Type type)
//		{
//			return JsvReader.GetParseFn(type) != null;
//		}

//		/// <summary>
//		/// Parses the specified value.
//		/// 
//		/// </summary>
//		/// <param name="value">The value.</param>
//		/// <returns/>
//		public static T DeserializeFromString<T>(string value)
//		{
//			if (string.IsNullOrEmpty(value))
//				return default(T);
//			return (T)JsvReader<T>.Parse(value);
//		}

//		public static T DeserializeFromReader<T>(TextReader reader)
//		{
//			return TypeSerializer.DeserializeFromString<T>(reader.ReadToEnd());
//		}

//		/// <summary>
//		/// Parses the specified type.
//		/// 
//		/// </summary>
//		/// <param name="type">The type.</param><param name="value">The value.</param>
//		/// <returns/>
//		public static object DeserializeFromString(string value, Type type)
//		{
//			if (value != null)
//				return JsvReader.GetParseFn(type)(value);
//			return (object)null;
//		}

//		public static object DeserializeFromReader(TextReader reader, Type type)
//		{
//			return TypeSerializer.DeserializeFromString(reader.ReadToEnd(), type);
//		}

//		public static string SerializeToString<T>(T value)
//		{
//			if ((object)value == null || (object)value is Delegate)
//				return (string)null;
//			if (typeof(T) == typeof(string))
//				return (object)value as string;
//			if (typeof(T) == typeof(object) || PlatformExtensions.IsAbstract(typeof(T)) || PlatformExtensions.IsInterface(typeof(T)))
//			{
//				if (PlatformExtensions.IsAbstract(typeof(T)) || PlatformExtensions.IsInterface(typeof(T)))
//					JsState.IsWritingDynamic = true;
//				string str = TypeSerializer.SerializeToString((object)value, value.GetType());
//				if (PlatformExtensions.IsAbstract(typeof(T)) || PlatformExtensions.IsInterface(typeof(T)))
//					JsState.IsWritingDynamic = false;
//				return str;
//			}
//			StringBuilder sb = new StringBuilder();
//			using (StringWriter stringWriter = new StringWriter(sb, (IFormatProvider)CultureInfo.InvariantCulture))
//				JsvWriter<T>.WriteRootObject((TextWriter)stringWriter, (object)value);
//			return sb.ToString();
//		}

//		public static string SerializeToString(object value, Type type)
//		{
//			if (value == null)
//				return (string)null;
//			if (type == typeof(string))
//				return value as string;
//			StringBuilder sb = new StringBuilder();
//			using (StringWriter stringWriter = new StringWriter(sb, (IFormatProvider)CultureInfo.InvariantCulture))
//				JsvWriter.GetWriteFn(type)((TextWriter)stringWriter, value);
//			return sb.ToString();
//		}

//		public static void SerializeToWriter<T>(T value, TextWriter writer)
//		{
//			if ((object)value == null)
//				return;
//			if (typeof(T) == typeof(string))
//				writer.Write((object)value);
//			else if (typeof(T) == typeof(object))
//			{
//				if (PlatformExtensions.IsAbstract(typeof(T)) || PlatformExtensions.IsInterface(typeof(T)))
//					JsState.IsWritingDynamic = true;
//				TypeSerializer.SerializeToWriter((object)value, value.GetType(), writer);
//				if (!PlatformExtensions.IsAbstract(typeof(T)) && !PlatformExtensions.IsInterface(typeof(T)))
//					return;
//				JsState.IsWritingDynamic = false;
//			}
//			else
//				JsvWriter<T>.WriteRootObject(writer, (object)value);
//		}

//		public static void SerializeToWriter(object value, Type type, TextWriter writer)
//		{
//			if (value == null)
//				return;
//			if (type == typeof(string))
//				writer.Write(value);
//			else
//				JsvWriter.GetWriteFn(type)(writer, value);
//		}

//		public static void SerializeToStream<T>(T value, Stream stream)
//		{
//			if ((object)value == null)
//				return;
//			if (typeof(T) == typeof(object))
//			{
//				if (PlatformExtensions.IsAbstract(typeof(T)) || PlatformExtensions.IsInterface(typeof(T)))
//					JsState.IsWritingDynamic = true;
//				TypeSerializer.SerializeToStream((object)value, value.GetType(), stream);
//				if (!PlatformExtensions.IsAbstract(typeof(T)) && !PlatformExtensions.IsInterface(typeof(T)))
//					return;
//				JsState.IsWritingDynamic = false;
//			}
//			else
//			{
//				StreamWriter streamWriter = new StreamWriter(stream, (Encoding)TypeSerializer.UTF8EncodingWithoutBom);
//				JsvWriter<T>.WriteRootObject((TextWriter)streamWriter, (object)value);
//				streamWriter.Flush();
//			}
//		}

//		public static void SerializeToStream(object value, Type type, Stream stream)
//		{
//			StreamWriter streamWriter = new StreamWriter(stream, (Encoding)TypeSerializer.UTF8EncodingWithoutBom);
//			JsvWriter.GetWriteFn(type)((TextWriter)streamWriter, value);
//			streamWriter.Flush();
//		}

//		public static T Clone<T>(T value)
//		{
//			return TypeSerializer.DeserializeFromString<T>(TypeSerializer.SerializeToString<T>(value));
//		}

//		public static T DeserializeFromStream<T>(Stream stream)
//		{
//			using (StreamReader streamReader = new StreamReader(stream, (Encoding)TypeSerializer.UTF8EncodingWithoutBom))
//				return TypeSerializer.DeserializeFromString<T>(streamReader.ReadToEnd());
//		}

//		public static object DeserializeFromStream(Type type, Stream stream)
//		{
//			using (StreamReader streamReader = new StreamReader(stream, (Encoding)TypeSerializer.UTF8EncodingWithoutBom))
//				return TypeSerializer.DeserializeFromString(streamReader.ReadToEnd(), type);
//		}

//		/// <summary>
//		/// Useful extension method to get the Dictionary[string,string] representation of any POCO type.
//		/// 
//		/// </summary>
//		/// 
//		/// <returns/>
//		public static Dictionary<string, string> ToStringDictionary<T>(this T obj)
//		{
//			return TypeSerializer.DeserializeFromString<Dictionary<string, string>>(TypeSerializer.SerializeToString<T>(obj));
//		}

//		/// <summary>
//		/// Recursively prints the contents of any POCO object in a human-friendly, readable format
//		/// 
//		/// </summary>
//		/// 
//		/// <returns/>
//		public static string Dump<T>(this T instance)
//		{
//			return TypeSerializer.SerializeAndFormat<T>(instance);
//		}

//		/// <summary>
//		/// Print Dump to Console.WriteLine
//		/// 
//		/// </summary>
//		public static void PrintDump<T>(this T instance)
//		{
//			Console.WriteLine(TypeSerializer.SerializeAndFormat<T>(instance));
//		}

//		/// <summary>
//		/// Print string.Format to Console.WriteLine
//		/// 
//		/// </summary>
//		public static void Print(this string text, params object[] args)
//		{
//			if (args.Length > 0)
//				Console.WriteLine(text, args);
//			else
//				Console.WriteLine(text);
//		}

//		public static string SerializeAndFormat<T>(this T instance)
//		{
//			Delegate fn = (object)instance as Delegate;
//			if (fn != null)
//				return TypeSerializer.Dump(fn);
//			return JsvFormatter.Format(TypeSerializer.SerializeToString<T>(instance));
//		}

//		public static string Dump(this Delegate fn)
//		{
//			MethodInfo method = fn.GetType().GetMethod("Invoke");
//			StringBuilder stringBuilder = new StringBuilder();
//			foreach (ParameterInfo parameterInfo in method.GetParameters())
//			{
//				if (stringBuilder.Length > 0)
//					stringBuilder.Append(", ");
//				stringBuilder.AppendFormat("{0} {1}", (object)parameterInfo.ParameterType.Name, (object)parameterInfo.Name);
//			}
//			string name = fn.Method.Name;
//			return StringExtensions.Fmt("{0} {1}({2})", (object)method.ReturnType.Name, (object)name, (object)stringBuilder);
//		}
//	}
//}
