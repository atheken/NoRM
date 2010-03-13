using NoRM.BSON;

namespace NoRM
{
    public class PullAllOperation<T> : ModifierCommand
    {
        public PullAllOperation(params T[] values): base("$pullAll", values)
        {
        }
    }
}