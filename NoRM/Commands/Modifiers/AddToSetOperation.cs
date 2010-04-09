namespace Norm.Commands.Modifiers
{
    using BSON;


    public class AddToSetOperation<T>: ModifierCommand
    {
        public AddToSetOperation(T addToSetValue)
            : base("$addToSet",addToSetValue)
        {
        }
    }
}