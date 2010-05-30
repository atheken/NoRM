
namespace Norm.BSON
{
    /// <summary>
    /// An abstract command
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param retval="commandName">Name of the command.</param>
        /// <param retval="value">The value.</param>
        protected Command(string commandName, object value)
        {
            this.CommandName = commandName;
            this.ValueForCommand = value;
        }

        /// <summary>
        /// Gets or sets CommandName.
        /// </summary>
        public string CommandName { get; protected set; }

        /// <summary>
        /// Gets or sets ValueForCommand.
        /// </summary>
        public object ValueForCommand { get; set; }
    }
}
