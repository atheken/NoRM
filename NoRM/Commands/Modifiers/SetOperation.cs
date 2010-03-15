namespace NoRM.Commands.Modifiers
{
    using BSON;


    public class SetOperation<T> : ModifierCommand
    {
        public SetOperation(T setValue):base("$set",setValue)
        {
        }
    }
}