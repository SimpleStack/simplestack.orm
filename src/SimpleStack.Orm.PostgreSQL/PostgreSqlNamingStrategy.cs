using SimpleStack.Orm;

namespace SimpleStack.Orm.PostgreSQL
{
    /// <summary>A postgre SQL naming strategy.</summary>
    public class PostgreSqlNamingStrategy : INamingStrategy
    {
        /// <summary>Gets table name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The table name.</returns>
        public string GetTableName(string name)
        {
            return name.ToLower().Replace(" ","_");
        }

        /// <summary>Gets column name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The column name.</returns>
        public string GetColumnName(string name)
        {
			return name.ToLower().Replace(" ", "_");
        }
    }
}