namespace Norm.Commands.Modifiers
{
    using BSON;

    /// <summary>TODO::Description.</summary>
    public class PopOperation : ModifierCommand
    {
        /// <summary>TODO::Description.</summary>
        public PopOperation(PopType popType) : base("$pop",(int) popType)
        {
        }
    }

    /// <summary>TODO::Description.</summary>
    public enum PopType
    {
        /// <summary>TODO::Description.</summary>
        RemoveFirst = -1,
        /// <summary>TODO::Description.</summary>
        RemoveLast = 1
    }
}