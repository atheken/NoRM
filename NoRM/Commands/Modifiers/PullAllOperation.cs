namespace NoRM
{
    using BSON;


    public class PullAllOperation<T> : ModifierCommand
    {
        public PullAllOperation(params T[] values): base("$pullAll", values)
        {
        }
    }
}