using SimpleStack.Orm.Attributes;

namespace SimpleStack.Orm.Tests
{
	public class TestType
	{
		/// <summary>Gets or sets the int column.</summary>
		/// <value>The int column.</value>
		public int IntColumn { get; set; }

		/// <summary>Gets or sets a value indicating whether the column.</summary>
		/// <value>true if column, false if not.</value>
		public bool BoolColumn { get; set; }

		/// <summary>Gets or sets the string column.</summary>
		/// <value>The string column.</value>
		public string StringColumn { get; set; }

		/// <summary>Gets or sets the nullable col.</summary>
		/// <value>The nullable col.</value>
		public TestType2 NullableCol { get; set; }

		/// <summary>Gets or sets the identifier.</summary>
		/// <value>The identifier.</value>
		[AutoIncrement]
		public int Id { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
		/// <see cref="T:System.Object" />.
		/// </summary>
		/// <param name="other">The test type to compare to this object.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object" /> is equal to the current
		/// <see cref="T:System.Object" />; otherwise, false.
		/// </returns>
		public bool Equals(TestType other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.IntColumn == IntColumn && other.BoolColumn.Equals(BoolColumn) && Equals(other.StringColumn, StringColumn);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
		/// <see cref="T:System.Object" />.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object" /> is equal to the current
		/// <see cref="T:System.Object" />; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(TestType)) return false;
			return Equals((TestType)obj);
		}

		/// <summary>Serves as a hash function for a particular type.</summary>
		/// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int result = IntColumn;
				result = (result * 397) ^ BoolColumn.GetHashCode();
				result = (result * 397) ^ (StringColumn != null ? StringColumn.GetHashCode() : 0);
				return result;
			}
		}
	}
}