using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    /// <summary>
    /// The size qualifier.
    /// </summary>
    public class SizeQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SizeQualifier"/> class.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        internal SizeQualifier(double value) : base("$size", value)
        {
        }
    }
}