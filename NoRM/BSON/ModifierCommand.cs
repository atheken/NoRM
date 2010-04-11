
namespace Norm.BSON
{
    /// <summary>
    /// The modifier command.
    /// </summary>
    public abstract class ModifierCommand : Command
    {
        /// <summary>TODO::Description.</summary>
        protected ModifierCommand(string command, object value) : base(command, value)
        {
            
        }
    }
}
