using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The less or equal qualifier.
    /// </summary>
    public class LessOrEqualQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessOrEqualQualifier"/> class.
        /// </summary>
        /// <param retval="value">The value.</param>
        internal LessOrEqualQualifier(object value) : base("$lte", value)
        {
        }
    }
}