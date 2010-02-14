using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;
using System.IO;
using System.Data.Mongo;
using System.Text.RegularExpressions;

namespace BSONHarness
{
    class Program
    {
        
        static void Main(string[] args)
        {
            InsertFindDeleteBenchmark(1);
            InsertFindDeleteBenchmark(100);
            InsertFindDeleteBenchmark(1000);
            InsertFindDeleteBenchmark(10000);
            InsertFindDeleteBenchmark(50000);

            SerializationBenchmark(1);
            SerializationBenchmark(100);
            SerializationBenchmark(1000);
            SerializationBenchmark(10000);
            SerializationBenchmark(50000);

            AuthenticateAConnection();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void AuthenticateAConnection()
        {
            var auth = new MongoContext().Authenticate("testing", "testing");
        }

        /// <summary>
        /// Inserts, finds one from the middle of the batch, then deletes from the specified collection.
        /// </summary>
        /// <param name="count"></param>
        private static void InsertFindDeleteBenchmark(int count)
        {
            MongoContext context = new MongoContext();
            var coll = context.GetDatabase("benchmark").GetCollection<DatabaseInfo>("dbinfo");
            coll.Delete(new { });

            BSONOID oid = BSONOID.EMPTY;
            DateTime start = DateTime.Now;

            List<DatabaseInfo> cache = new List<DatabaseInfo>(count);
            int toUse = (int)Math.Floor(count / 2f);
            for (int i = 0; i < count; i++)
            {
                var into = new DatabaseInfo();
                into.Name = "DBTest";
                into.SizeOnDisk = i;
                cache.Add(into);
            }
            coll.Insert(cache);
            Console.WriteLine("Inserted {0} objects in {1}ms", count,
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            var first = coll.Find(new { SizeOnDisk = toUse }).First();
            Console.WriteLine("   Search for 1 object in {1}ms", count,
                (DateTime.Now - start).TotalMilliseconds);

            
            start = DateTime.Now;
            coll.UpdateOne(new { SizeOnDisk = toUse }, new { SizeOnDisk = M.Inc(5) });
            Console.WriteLine("   Updated that one in {0}ms",
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            first = coll.Find(new { SizeOnDisk = toUse += 5 }).First();

            coll.Delete(new { SizeOnDisk = Q.LessThan(Double.MaxValue) });
            Console.WriteLine("   Deleted {0} objects in {1}ms\r\n", count,
                (DateTime.Now - start).TotalMilliseconds);
        }

        /// <summary>
        /// Tests the speed of the BSON serializer/deserializer.
        /// </summary>
        /// <param name="count"></param>
        private static void SerializationBenchmark(int count)
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                var into = new DatabaseInfo();
                into.Name = "DB1";
                into.SizeOnDisk = 3829383298;
                var bytes = BSONSerializer.Serialize(into);

                var outTo = BSONSerializer.Deserialize<DatabaseInfo>(bytes);
            }
            Console.WriteLine("Serialized/deserialized {0} objects in {1}ms", count, (DateTime.Now - now).TotalMilliseconds);
        }
    }
}
