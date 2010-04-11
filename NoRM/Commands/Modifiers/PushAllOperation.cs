namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class PushAllOperation<T> : ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public PushAllOperation(params T[] pushValues):base("$pushAll",pushValues)
        {
        }
    }
}