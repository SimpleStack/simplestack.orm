namespace SimpleStack.Orm
{
    /// <summary>Interface for naming strategy.</summary>
    public interface INamingStrategy
    {
        /// <summary>Gets table name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The table name.</returns>
        string GetTableName(string name);

        /// <summary>Gets column name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The column name.</returns>
        string GetColumnName(string name);
    }
}