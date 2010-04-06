namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class PushOperation<T> : ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public PushOperation(T pushValue):base("$push",pushValue)
        {
        }
    }
}