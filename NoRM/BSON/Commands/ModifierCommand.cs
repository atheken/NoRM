using System;

namespace NoRM.BSON
{
    /// <summary>
    /// Represents a mongo modifier command 
    /// </summary>
    public abstract class ModifierCommand : Command
    {
        protected ModifierCommand(string commandName, object valueForCommand)
        {
            CommandName = commandName;
            ValueForCommand = valueForCommand;
        }
    }
}
