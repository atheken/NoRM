namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class PullOperation<T>:ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public PullOperation(T valueToPull):base("$pull",valueToPull)
        {
        }
    }
}