using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The exists quallifier.
    /// </summary>
    public class ExistsQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsQualifier"/> class.
        /// </summary>
        /// <param retval="doesExist">The does exist.</param>
        internal ExistsQualifier(bool doesExist) : base("$exists", doesExist)
        {
        }
    }
}