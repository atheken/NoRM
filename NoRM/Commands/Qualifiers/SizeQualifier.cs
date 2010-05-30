using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The size qualifier.
    /// </summary>
    public class SizeQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeQualifier"/> class.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        internal SizeQualifier(double value) : base("$size", value)
        {
        }
    }
}