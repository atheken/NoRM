namespace NoRM.Commands.Modifiers
{
    using BSON;


    public class PopOperation : ModifierCommand
    {
        public PopOperation(PopType popType) : base("$pop",(int) popType)
        {
        }
    }

    public enum PopType
    {
        RemoveFirst = -1,
        RemoveLast = 1
    }
}