using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NoRM;
using NoRM.BSON;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.BSON.DbTypes;

namespace BSONHarness
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var server = new MongoContext();
            //server.Connect();

            var s = server.ServerStatus();

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
            var db = context.GetDatabase("benchmark");
            db.DropCollection("test");
            var coll = db.GetCollection<GeneralDTO>("test");
            coll.CreateIndex(new { Int = 1d }, false, "testIdx");

            OID oid = OID.EMPTY;
            DateTime start = DateTime.Now;

            List<GeneralDTO> cache = new List<GeneralDTO>(count);
            int toUse = (int)Math.Floor(count / 2f);
            for (int i = 0; i < count; i++)
            {
                var into = new GeneralDTO();
                into._id = OID.NewOID();
                into.Title = Guid.NewGuid().ToString();
                into.Int = i;
                if (i == toUse)
                {
                    oid = into._id;
                }
                cache.Add(into);
            }
            coll.Insert(cache);
            Console.WriteLine("Inserted {0} objects in {1}ms", count,
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            var first = coll.Find(new { _id = oid }).First();
            Console.WriteLine("   Search for 1 object in {1}ms", count,
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            //find something randomly using a regex.
            var numLessThan2 = coll.Find(new { Title = new Regex(".*8a.*", RegexOptions.IgnoreCase) }).ToArray().Count();
            Console.WriteLine("   Found {0} objects that match the regex. in {1}ms", numLessThan2,
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            coll.UpdateOne(new { _id = oid }, new { Int = M.Inc(5) });
            Console.WriteLine("   Updated that one in {0}ms",
                (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            first = coll.Find(new { _id = oid }).First();

            coll.Delete(new { Int = Q.LessThan(Int32.MaxValue) });
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
                var inTo = new GeneralDTO();
                var bytes = BSONSerializer.Serialize(inTo);

                var outTo = BSONSerializer.Deserialize<GeneralDTO>(bytes);
            }
            Console.WriteLine("Serialized/deserialized {0} objects in {1}ms", count, (DateTime.Now - now).TotalMilliseconds);
        }

        public class GeneralDTOWithNestedObject
        {
            public GeneralDTO DTO1 { get; set; }
        }

        /// <summary>
        /// A basic class definition that will be used for testing.
        /// </summary>
        public class GeneralDTO
        {
            public OID _id { get; set; }
            public int? Int { get; set; }
            public String Title { get; set; }
            public Regex Rex { get; set; }
            public List<DatabaseInfo> AList { get; set; }
        }
    }
}
