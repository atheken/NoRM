using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Commands;

namespace NoRM
{
    using Commands.Modifiers;


    /// <summary>
    /// Shorthand to construct modifier operations
    /// for MongoDB fields.
    /// </summary>
    /// <remarks>
    /// This should remain in the System.Data.Mongo namespace so that it's available 
    /// automatically when someone is using a MongoCollection.
    /// </remarks>
    public class M
    {
        /// <summary>
        /// Creates a $inc operation to be applied to a field using the update command.
        /// </summary>
        /// <param name="amountToIncrementBy"></param>
        /// <returns></returns>
        public static IncrementOperation Inc(int amountToIncrementBy)
        {
            return new IncrementOperation(amountToIncrementBy);
        }
        ///<summary>
        /// Creates a $set operation to be applied to a field using the update command. 
        ///</summary>
        ///<param name="valueToSet"></param>
        ///<returns></returns>
        public static SetOperation<T> Set<T>(T setValue)
        {
            return new SetOperation<T>(setValue);
        }
        public static PushOperation<T> Push<T>(T pushValue)
        {
            return new PushOperation<T>(pushValue);
        }
        public static PushAllOperation<T> PushAll<T>(params T[] pushValues)
        {
            return new PushAllOperation<T>(pushValues);
        }

        public static AddToSetOperation<T> AddToSet<T>(T addToSetValue)
        {
            return new AddToSetOperation<T>(addToSetValue);
        }
        public static PullOperation<T> Pull<T>(T pullValue)
        {
            return new PullOperation<T>(pullValue);
        }
        public static PopOperation Pop(PopType popType)
        {
            return new PopOperation(popType);
        }
        public static PullAllOperation<T> PullAll<T>(params T[] pullValues)
        {
            return new PullAllOperation<T>(pullValues);
        }
    }
}
