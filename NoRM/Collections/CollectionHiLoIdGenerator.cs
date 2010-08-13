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
    public class CollectionHiLoIdGenerator
    {
        private readonly int _capacity;
        private readonly object generatorLock = new object();
        private IDictionary<string, HiLoIdGenerator> keyGeneratorsByTag = new Dictionary<string, HiLoIdGenerator>();

        public CollectionHiLoIdGenerator(int capacity)
        {
            _capacity = capacity;
        }

        /// <summary>
        /// Generates the identity value
        /// </summary>
        /// <param name="db">MongoDatabase instance</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>Generated identity</returns>
        public long GenerateId(IMongoDatabase db, string collectionName)
        {
            HiLoIdGenerator value;

            //if (keyGeneratorsByTag.TryGetValue(collectionName, out value))
            //    return value.GenerateId(collectionName);

            lock (generatorLock)
            {
                if (keyGeneratorsByTag.TryGetValue(collectionName, out value))
                    return value.GenerateId(collectionName, db);

                value = new HiLoIdGenerator(_capacity);
                // doing it this way for thread safety
                keyGeneratorsByTag = new Dictionary<string, HiLoIdGenerator>(keyGeneratorsByTag)
                {
                    {collectionName, value}
                };
            }

            return value.GenerateId(collectionName, db);
        }
    }
}
