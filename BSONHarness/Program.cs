using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;
using System.IO;
using System.Data.Mongo;

namespace BSONHarness
{
    class Program
    {
        public class GeneralDTO
        {
            public GeneralDTO()
            {
            }
            public BSONOID _id { get; set; }
            public String Title { get; set; }
            //public int? Random { get; set; }
        }

        static void Main(string[] args)
        {
            MongoContext context = new MongoContext();

            var collection = context.GetDatabase("delta").GetCollection<GeneralDTO>("gamma");

            int count = 10000;
            List<GeneralDTO> addList = new List<GeneralDTO>();
            for (int i = 0; i < count; i++)
            {
                addList.Add(new GeneralDTO() { Title = "C", _id = BSONOID.NewOID() });
            }

            var start = DateTime.Now;
            collection.Insert(addList);
            Console.WriteLine("Wrote {0} in {1:s} seconds", count, DateTime.Now - start);
            start = DateTime.Now;
            
            var resultCount = collection.Find(new { Title = "C" }).Count();

            Console.WriteLine("Then queried and found {0} in {1:s} seconds.", resultCount, DateTime.Now - start);

            //SerializationBenchmark(1);
            //SerializationBenchmark(100);
            //SerializationBenchmark(1000);
            //SerializationBenchmark(10000);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void SerializationBenchmark(int count)
        {
            BSONSerializer serializer = new BSONSerializer();
            DateTime now = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                var inTo = new GeneralDTO();
                var bytes = serializer.Serialize(inTo);
                var ms = new MemoryStream();
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                var outTo = serializer.Deserialize<GeneralDTO>(new BinaryReader(ms));
            }
            Console.WriteLine("Constructed, serialized and then deserialized {0} objects in {1:s} seconds",
                count, (DateTime.Now - now));
        }
    }
}
