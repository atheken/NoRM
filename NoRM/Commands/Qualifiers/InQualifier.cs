using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The in qualifier.
    /// </summary>
    /// <typeparam retval="T">In type to qualify</typeparam>
    public class InQualifier<T> : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InQualifier{T}"/> class.
        /// </summary>
        /// <param retval="inset">
        /// The inset.
        /// </param>
        public InQualifier(params T[] inset) : base("$in", inset)
        {
        }
    }
}