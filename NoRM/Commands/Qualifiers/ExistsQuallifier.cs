using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The exists quallifier.
    /// </summary>
    public class ExistsQuallifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsQuallifier"/> class.
        /// </summary>
        /// <param name="doesExist">The does exist.</param>
        internal ExistsQuallifier(bool doesExist) : base("$exists", doesExist)
        {
        }
    }
}