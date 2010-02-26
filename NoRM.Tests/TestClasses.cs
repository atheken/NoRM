using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;
using NoRM.BSON.DbTypes;
using System.Text.RegularExpressions;

namespace NoRM.Tests
{
    public class TestClass
    {
        public TestClass()
        {
            this.TestClassID = Guid.NewGuid();
        }
        [MongoIdentifier]
        public Guid? TestClassID { get; set; }
        public double? ADouble { get; set; }
        public string AString { get; set; }
        public int? AInteger { get; set; }
        public List<String> AStringArray { get; set; }
    }

    public enum Flags32
    {
        FlagNone = 0,
        FlagOn = 1,
        FlagOff = 2
    }

    public enum Flags64 : long
    {
        FlagNone = 0,
        FlagOn = 1,
        FlagOff = 2
    }

    public class MiniObject
    {
        public OID _id { get; set; }
    }

    public class GeneralDTO
    {
        public double? Pi { get; set; }
        public int? AnInt { get; set; }
        public String Title { get; set; }
        public bool? ABoolean { get; set; }
        public byte[] Bytes { get; set; }
        public Guid? AGuid { get; set; }
        public Regex ARex { get; set; }
        public DateTime? ADateTime { get; set; }
        public GeneralDTO Nester { get; set; }
        public ScopedCode Code { get; set; }
        public Flags32? Flags32 { get; set; }
        public Flags64? Flags64 { get; set; }

        [MongoIgnore]
        public int IgnoredProperty { get; set; }
    }
}
