using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    /// <summary>
    /// The greater or equal qualifier.
    /// </summary>
    public class GreaterOrEqualQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterOrEqualQualifier"/> class.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        internal GreaterOrEqualQualifier(double value) : base("$gte", value)
        {
        }
    }
}