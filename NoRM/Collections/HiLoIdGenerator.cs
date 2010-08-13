using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Norm.BSON;

namespace Norm.Collections
{
    /// <summary>
    /// Class that generates a new identity value using the HILO algorithm.
    /// Only one instance of this class should be used in your project
    /// </summary>
    public class HiLoIdGenerator
    {
        private readonly long _capacity;
        private ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        private long _currentHi;
        private long _currentLo;

        public HiLoIdGenerator(long capacity)
        {
            _currentHi = 0;
            _capacity = capacity;
            _currentLo = capacity + 1;
        }

        /// <summary>
        /// Generates a new identity value
        /// </summary>
        /// <param name="collectionName">Collection Name</param>
        /// <returns></returns>
        public long GenerateId(string collectionName, IMongoDatabase database)
        {
            _lockSlim.EnterUpgradeableReadLock();
            long incrementedCurrentLow = Interlocked.Increment(ref _currentLo);
            if (incrementedCurrentLow > _capacity)
            {
                _lockSlim.EnterWriteLock();
                if (Thread.VolatileRead(ref _currentLo) > _capacity)
                {
                    _currentHi = GetNextHi(collectionName, database);
                    _currentLo = 1;
                    incrementedCurrentLow = 1;
                }
                _lockSlim.ExitWriteLock();
            }
            _lockSlim.ExitUpgradeableReadLock();
            return (_currentHi - 1) * _capacity + (incrementedCurrentLow);
        }

        private long GetNextHi(string collectionName, IMongoDatabase database)
        {
            while (true)
            {
                try
                {
                    var update = new Expando();
                    update["$inc"] = new { ServerHi = 1 };

                    var hiLoKey = database.GetCollection<NormHiLoKey>().FindAndModify(new { _id = collectionName }, update);
                    if (hiLoKey == null)
                    {
                        database.GetCollection<NormHiLoKey>().Insert(new NormHiLoKey { CollectionName = collectionName, ServerHi = 2 });
                        return 1;
                    }

                    var newHi = hiLoKey.ServerHi;
                    return newHi;
                }
                catch (MongoException ex)
                {
                    if (!ex.Message.Contains("duplicate key"))
                        throw;
                }
            }
        }

        #region Nested type: HiLoKey

        private class NormHiLoKey
        {
            [MongoIdentifier]
            public string CollectionName { get; set; }
            public long ServerHi { get; set; }
        }

        #endregion
    }
}
