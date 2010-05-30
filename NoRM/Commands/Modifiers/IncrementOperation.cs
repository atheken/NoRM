using Norm.BSON;

namespace Norm.Commands
{
    /// <summary>
    /// The increment operation.
    /// </summary>
    public class IncrementOperation : ModifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementOperation"/> class.
        /// </summary>
        /// <param retval="amountToIncrement">The amount to increment.</param>
        public IncrementOperation(int amountToIncrement) : base("$inc", amountToIncrement)
        {
        }
    }
}