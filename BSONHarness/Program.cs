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
            GeneralDTO gto = new GeneralDTO() { Rex = new Regex("[0-9]{6}", RegexOptions.ExplicitCapture | RegexOptions.Compiled) };
            var x = BSONSerializer.Deserialize<GeneralDTO>(BSONSerializer.Serialize(gto));

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

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Inserts, finds one from the middle of the batch, then deletes from the specified collection.
        /// </summary>
        /// <param name="count"></param>
        private static void InsertFindDeleteBenchmark(int count)
        {
            MongoContext context = new MongoContext();
            var coll = context.GetDatabase("benchmark").GetCollection<GeneralDTO>("test");
            coll.Delete(new { });
            
            BSONOID oid = BSONOID.EMPTY;
            DateTime now = DateTime.Now;

            List<GeneralDTO> cache = new List<GeneralDTO>(count);
            int toUse = (int)Math.Floor(count / 2f);
            for (int i = 0; i < count; i++)
            {
                var into = new GeneralDTO();
                into._id = BSONOID.NewOID();
                into.Title = "ABCDEFG";
                into.Incremental = 1;
                if (i == toUse)
                {
                    oid = into._id;
                }
                cache.Add(into);
            }
            coll.Insert(cache);
            Console.WriteLine("Inserted {0} objects in {1}ms", count,
                (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;

            var first = coll.Find(new { _id = oid }).First();
            Console.WriteLine("   Search for 1 object in {1}ms", count,
                (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;

            coll.UpdateOne(new { _id = oid }, new { Incremental = M.Inc(5) });
            Console.WriteLine("   Updated that 1 object in {1}ms", count,
                (DateTime.Now - now).TotalMilliseconds);
            now = DateTime.Now;

            first = coll.Find(new { _id = oid }).First();

            coll.Delete(new {});
            Console.WriteLine("   Deleted {0} objects in {1}ms\r\n", count,
                (DateTime.Now - now).TotalMilliseconds);
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
                var inTo = new GeneralDTO();
                var bytes = BSONSerializer.Serialize(inTo);

                var outTo = BSONSerializer.Deserialize<GeneralDTO>(bytes);
            }
            Console.WriteLine("Serialized/deserialized {0} objects in {1}ms", count, (DateTime.Now - now).TotalMilliseconds);
        }

        /// <summary>
        /// A basic class definition that will be used for testing.
        /// </summary>
        public class GeneralDTO
        {
            public BSONOID _id { get; set; }
            public int? Incremental { get; set; }
            public String Title { get; set; }
            public Regex Rex { get; set; }
        }
    }
}
