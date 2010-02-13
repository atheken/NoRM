using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo
{
    public class DatabaseInfo
    {
        public DatabaseInfo() { }

        public string name { get; set; }
        public double? sizeOnDisk { get; set; }
    }
}
