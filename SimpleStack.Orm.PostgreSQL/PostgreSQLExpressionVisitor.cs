using SimpleStack.Orm.Expressions;

namespace NServiceKit.OrmLite.PostgreSQL
{
    /// <summary>A postgre SQL expression visitor.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
	public class PostgreSQLExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
        /// <summary>Gets the limit expression.</summary>
        /// <value>The limit expression.</value>
		public override string LimitExpression{
			get{
				if(!Rows.HasValue) return "";
				string offset;
				if(Skip.HasValue){
					offset= string.Format(" OFFSET {0}", Skip.Value );
				}
				else{
					offset=string.Empty;
				}
				return string.Format("LIMIT {0}{1}", Rows.Value, offset);                   
			}
		}
		
	}
}