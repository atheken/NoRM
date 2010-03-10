
namespace NoRM.BSON
{
    /// <summary>
    /// An abstract command
    /// </summary>
    public abstract class Command
    {
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
