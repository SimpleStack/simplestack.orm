using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleStack.Orm.Expressions
{
    /// <summary>
    /// http://blogs.msdn.com/b/meek/archive/2008/05/02/linq-to-entities-combining-predicates.aspx.
    /// </summary>
    public class ParameterRebinder : ExpressionVisitor
    {
        /// <summary>The map.</summary>
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        /// <summary>
        /// Initializes a new instance of the NServiceKit.OrmLite.ParameterRebinder class.
        /// </summary>
        /// <param name="map">The map.</param>
        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        /// <summary>Replace parameters.</summary>
        /// <param name="map">The map.</param>
        /// <param name="exp">The exponent.</param>
        /// <returns>An Expression.</returns>
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        /// <summary>Visit parameter.</summary>
        /// <param name="p">The ParameterExpression to process.</param>
        /// <returns>An Expression.</returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
    }
}