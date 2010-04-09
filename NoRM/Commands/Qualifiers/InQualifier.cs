using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The in qualifier.
    /// </summary>
    /// <typeparam name="T">In type to qualify</typeparam>
    public class InQualifier<T> : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InQualifier{T}"/> class.
        /// </summary>
        /// <param name="inset">
        /// The inset.
        /// </param>
        public InQualifier(params T[] inset) : base("$in", inset)
        {
        }
    }
}