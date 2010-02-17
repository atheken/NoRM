using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Commands;

namespace NoRM
{
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
    }
}
