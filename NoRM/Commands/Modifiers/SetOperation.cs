using NoRM.BSON;

namespace NoRM.Commands
{
    /// <summary>
    /// The increment operation.
    /// </summary>
    public class SetOperation : ModifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementOperation"/> class.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public SetOperation(object value)
            : base("$set", value)
        {
        }
    }
}