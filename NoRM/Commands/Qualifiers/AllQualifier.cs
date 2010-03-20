using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The all qualifier.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class AllQualifier<T> : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllQualifier{T}"/> class.
        /// </summary>
        /// <param name="all">The value.</param>
        public AllQualifier(params T[] all) : base("$all", all)
        {
        }
    }
}