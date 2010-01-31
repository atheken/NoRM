using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;
using System.IO;

namespace BSONHarness
{
    class Program
    {
        public class GeneralDTO
        {
            public GeneralDTO()
            {
                //this.Random = DateTime.Now.Millisecond;
                //this.Title = this.Random.ToString();
                //this.IsFun = true;
            }
            public bool? IsFun { get; set; }
            public String Title { get; set; }
            public int? Random { get; set; }
        }

        static void Main(string[] args)
        {
            SerializationBenchmark(1);
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
