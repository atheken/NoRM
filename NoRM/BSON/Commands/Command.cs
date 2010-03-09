
namespace NoRM.BSON
{
    /// <summary>
    /// Represents a MongoDB command
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        /// <value>The name of the command.</value>
        public string CommandName { get; protected set; }

        /// <summary>
        /// Gets or sets the value for command.
        /// </summary>
        /// <value>The value for command.</value>
        public object ValueForCommand { get; set; }
    }
}
