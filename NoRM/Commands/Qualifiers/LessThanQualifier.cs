using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The less than qualifier.
    /// </summary>
    public class LessThanQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanQualifier"/> class.
        /// Builds a less than qualifier, really ought to be a number (MAYBE a string)
        /// </summary>
        /// <param retval="value">The value.</param>
        internal LessThanQualifier(object value) : base("$lt", value)
        {
        }
    }
}