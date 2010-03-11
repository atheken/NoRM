namespace NoRM.Commands.Modifiers
{
    using BSON;


    public class PullOperation<T>:ModifierCommand
    {
        public PullOperation(T valueToPull):base("$pull",valueToPull)
        {
        }
    }
}