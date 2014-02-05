using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;

namespace Norm.Collections
{
    /// <summary>
    /// Generates an identity using the HiLo algorithm per collection
    /// </summary>
    public class CollectionSequenceIdGenerator
    {
        private readonly object _generatorLock = new object();
		private IDictionary<string, SequenceIdGenerator> _keyGeneratorsByTag = new Dictionary<string, SequenceIdGenerator>();

    	/// <summary>
    	/// Generates the identity value
    	/// </summary>
    	/// <param name="db">MongoDatabase instance</param>
    	/// <param name="collectionName">Collection Name</param>
    	/// <returns>Generated identity</returns>
    	public long GenerateId(IMongoDatabase db, string collectionName)
        {
			SequenceIdGenerator value;

            lock (_generatorLock)
            {
                if (_keyGeneratorsByTag.TryGetValue(collectionName, out value))
                    return value.GenerateId(collectionName, db);

				value = new SequenceIdGenerator();
                // doing it this way for thread safety
				_keyGeneratorsByTag = new Dictionary<string, SequenceIdGenerator>(_keyGeneratorsByTag)
                {
                    {collectionName, value}
                };
            }

            return value.GenerateId(collectionName, db);
        }
    }
}
