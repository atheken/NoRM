using System.Linq.Expressions;

namespace NoRM.Linq
{
    /// <summary>
    /// A mongo query.
    /// </summary>
    public interface IMongoQuery
    {
        /// <summary>
        /// Gets an expression.
        /// </summary>
        /// <returns>
        /// </returns>
        Expression GetExpression();
    }
}