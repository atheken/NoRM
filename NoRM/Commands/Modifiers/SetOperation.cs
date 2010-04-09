namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class SetOperation<T> : ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public SetOperation(T setValue):base("$set",setValue)
        {
        }
    }
}