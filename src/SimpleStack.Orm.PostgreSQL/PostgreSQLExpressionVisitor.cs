using System.Linq.Expressions;
using SimpleStack.Orm.Expressions;

namespace SimpleStack.Orm.PostgreSQL
{
    /// <summary>A postgre SQL expression visitor.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
	public class PostgreSQLExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
	    public PostgreSQLExpressionVisitor(IDialectProvider dialectProvider) : base(dialectProvider)
	    {
	    }

	    protected override string BindOperant(ExpressionType e)
	    {
	        switch (e)
	        {
                case ExpressionType.ExclusiveOr:
                    return "#";
            }

            return base.BindOperant(e);
	    }

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