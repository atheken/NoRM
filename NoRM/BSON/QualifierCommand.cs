
namespace NoRM.BSON
{
    /// <summary>
    /// The qualifier command.
    /// </summary>
    public abstract class QualifierCommand : Command
    {
        protected QualifierCommand(string command, object value) : base(command, value)
        {
        }
    }
}