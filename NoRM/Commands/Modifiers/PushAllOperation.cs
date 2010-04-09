namespace Norm.Commands.Modifiers
{
    using BSON;


    public class PushAllOperation<T> : ModifierCommand
    {
        public PushAllOperation(params T[] pushValues):base("$pushAll",pushValues)
        {
        }
    }
}