
namespace Norm.BSON
{
    /// <summary>
    /// The qualifier command.
    /// </summary>
    public abstract class QualifierCommand : Command
    {
        /// <summary>TODO::Description.</summary>
        protected QualifierCommand(string command, object value) : base(command, value)
        {
        }
    }
}