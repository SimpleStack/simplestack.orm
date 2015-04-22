//using System.Collections;
//using System.Text;

//namespace SimpleStack.Orm
//{
//	/// <summary>A SQL in values.</summary>
//	public class SqlInValues
//	{
//		/// <summary>The values.</summary>
//		private readonly IEnumerable values;

//		/// <summary>Gets the number of. </summary>
//		/// <value>The count.</value>
//		public int Count { get; private set; }

//		/// <summary>
//		/// Initializes a new instance of the NServiceKit.OrmLite.SqlInValues class.
//		/// </summary>
//		/// <param name="values">The values.</param>
//		public SqlInValues(IEnumerable values)
//		{
//			this.values = values;

//			if (values != null)

//				foreach (var value in values)
//					++Count;
//		}

//		/// <summary>Converts this object to a SQL in string.</summary>
//		/// <returns>This object as a string.</returns>
//		public string ToSqlInString()
//		{
//			if (Count == 0)
//				return "NULL";

//			return SqlJoin(values);
//		}

//		/// <summary>SQL join.</summary>
//		/// <param name="values">The values to act on.</param>
//		/// <returns>A string.</returns>
//		public static string SqlJoin(IEnumerable values)
//		{
//			var sb = new StringBuilder();
//			foreach (var value in values)
//			{
//				if (sb.Length > 0) sb.Append(",");
//				sb.Append(Config.DialectProvider.GetQuotedValue(value, value.GetType()));
//			}

//			return sb.ToString();
//		}
//	}
//}