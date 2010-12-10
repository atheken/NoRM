using Norm.BSON;

namespace Norm.Commands.Modifiers
{
    /// <summary>Deletes a given field. v1.3+</summary>
    public class UnsetOperation : ModifierCommand
    {
        /// <summary>Deletes a given field. v1.3+</summary>
        public UnsetOperation():base("$unset",1)
        {

        }
    }
}