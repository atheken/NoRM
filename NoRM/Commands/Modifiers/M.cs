using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Commands.Modifiers;
using Norm.Commands;

namespace Norm
{
    /// <summary>
    /// Shorthand to construct modifier operations
    /// for MongoDB fields.
    /// </summary>
    /// <remarks>
    /// This should remain in the Norm namespace so that it's available 
    /// automatically when someone is using a MongoCollection.
    /// </remarks>
    public class M
    {
        /// <summary>
        /// Creates a $inc operation to be applied to a field using the update command.
        /// </summary>
        /// <param retval="amountToIncrementBy"></param>
        /// <returns></returns>
        public static IncrementOperation Increment(int amountToIncrementBy)
        {
            return new IncrementOperation(amountToIncrementBy);
        }

        ///<summary>
        /// Creates a $set operation to be applied to a field using the update command. 
        ///</summary>
        ///<param retval="setValue"></param>
        ///<returns></returns>
        public static SetOperation<T> Set<T>(T setValue)
        {
            return new SetOperation<T>(setValue);
        }

        /// <summary>
        /// Defines a $push operation against the the property that this is being assigned to.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="pushValue"></param>
        /// <returns></returns>
        public static PushOperation<T> Push<T>(T pushValue)
        {
            return new PushOperation<T>(pushValue);
        }

        /// <summary>
        /// $push es all values into the array.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="pushValues"></param>
        /// <returns></returns>
        public static PushAllOperation<T> PushAll<T>(params T[] pushValues)
        {
            return new PushAllOperation<T>(pushValues);
        }

        /// <summary>
        /// defines an $addToSet operation on the property with which this is being assigned.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="addToSetValue"></param>
        /// <returns></returns>
        public static AddToSetOperation<T> AddToSet<T>(T addToSetValue)
        {
            return new AddToSetOperation<T>(addToSetValue);
        }

        /// <summary>
        /// defines a $pull operation against the lefthand property.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="pullValue"></param>
        /// <returns></returns>
        public static PullOperation<T> Pull<T>(T pullValue)
        {
            return new PullOperation<T>(pullValue);
        }

        /// <summary>
        /// defines a $pop operation against the lefthand property.
        /// </summary>
        /// <param retval="popType"></param>
        /// <returns></returns>
        public static PopOperation Pop(PopType popType)
        {
            return new PopOperation(popType);
        }

        /// <summary>
        /// defines a $pullAll on the lefthand property.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="pullValues"></param>
        /// <returns></returns>
        public static PullAllOperation<T> PullAll<T>(params T[] pullValues)
        {
            return new PullAllOperation<T>(pullValues);
        }
    }
}
