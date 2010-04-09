using Norm.BSON;

namespace Norm
{
    /// <summary>TODO::Description.</summary>
    public class PullAllOperation<T> : ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public PullAllOperation(params T[] values): base("$pullAll", values)
        {
        }
    }
}