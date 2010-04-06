namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class AddToSetOperation<T>: ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public AddToSetOperation(T addToSetValue)
            : base("$addToSet",addToSetValue)
        {
        }
    }
}