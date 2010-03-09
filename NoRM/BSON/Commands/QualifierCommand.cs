using System;

namespace NoRM.BSON
{
    /// <summary>
    /// Represents a mongo modifier command
    /// </summary>
    public abstract class QualifierCommand : Command
    {
        protected QualifierCommand(string commandName, object valueForCommand)
        {
            CommandName = commandName;
            ValueForCommand = valueForCommand;
        }
    }
}
