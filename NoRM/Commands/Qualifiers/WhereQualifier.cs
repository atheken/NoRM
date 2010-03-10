using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    /// <summary>
    /// The where qualifier.
    /// </summary>
    public class WhereQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhereQualifier"/> class.
        /// </summary>
        /// <param name="inExpression">The in expression.</param>
        public WhereQualifier(string inExpression) : base("$where", inExpression)
        {
        }
    }
}