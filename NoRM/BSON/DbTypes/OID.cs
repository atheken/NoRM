namespace NoRM
{
    using System;
    using BSON.DbTypes;

    public class OID
    {
        public OID()
        {
        }

        public OID(string value)
        {
            Value = DecodeHex(value);
        }
        internal OID(byte[] value)
        {
            Value = value;
        }

        /// <summary>
        /// Provides an empty OID (all zeros).
        /// </summary>
        public static OID EMPTY
        {
            get { return new OID(); }
        }

        /// <summary>
        /// A 12-byte unique identifier.
        /// </summary>
        public byte[] Value { get; set; }

        /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns></returns>
        public static OID NewOID()
        {
            //TODO: generate random-ish bits.
            return new OID {Value = OidGenerator.Generate()};            
        }

        public static bool TryParse(string value, out OID id)
        {
            id = EMPTY;
            if (value == null || value.Length != 24)
            {
                return false;
            }
            try
            {
                id = new OID(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        protected static byte[] DecodeHex(string val)
        {
            var chars = val.ToCharArray();
            var numberChars = chars.Length;            
            var bytes = new byte[numberChars/2];
            
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i/2] = Convert.ToByte(new string(chars, i, 2), 16);
            }
            return bytes;
        }
    }
}