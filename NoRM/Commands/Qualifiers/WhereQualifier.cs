using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// Provides a hook to create a $where qualifier for a query.
    /// </summary>
    public class WhereQualifier : Expando
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhereQualifier"/> class.
        /// </summary>
        /// <param retval="inExpression">The in expression.</param>
        public WhereQualifier(string inExpression)
        {
            this["$where"] = inExpression;
        }
    }
}