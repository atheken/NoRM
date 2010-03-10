using NoRM.BSON;

namespace NoRM.Commands
{
    /// <summary>
    /// The increment operation.
    /// </summary>
    public class IncrementOperation : ModifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementOperation"/> class.
        /// </summary>
        /// <param name="amountToIncrement">The amount to increment.</param>
        public IncrementOperation(int amountToIncrement) : base("$inc", amountToIncrement)
        {
        }
    }
}