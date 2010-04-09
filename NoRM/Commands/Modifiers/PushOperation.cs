namespace Norm.Commands.Modifiers
{
    using BSON;


    public class PushOperation<T> : ModifierCommand
    {
        public PushOperation(T pushValue):base("$push",pushValue)
        {
        }
    }
}