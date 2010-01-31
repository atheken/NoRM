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
            public String Ti { get; set; }
            public double? TestFloat { get; set; }
            public String Title { get; set; }
        }

        static void Main(string[] args)
        {
            BSONSerializer serializer = new BSONSerializer();
            GeneralDTO gto = new GeneralDTO();
            gto.Title = "H";
            gto.Ti = "I";
            gto.TestFloat = 42.3f;
            var bytes = serializer.Serialize(gto);
            var ms = new MemoryStream();
            ms.Write(bytes,0,bytes.Length);
            ms.Position = 0;

            var dto = serializer.Deserialize<GeneralDTO>(new BinaryReader(ms));


        }
    }
}
