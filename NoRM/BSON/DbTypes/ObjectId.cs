namespace NoRM
{
    using System;
    using BSON.DbTypes;

    public class ObjectId
    {
        public ObjectId()
        {
        }

        public ObjectId(string value)
        {
            Value = DecodeHex(value);
        }
        internal ObjectId(byte[] value)
        {
            Value = value;
        }

        /// <summary>
        /// Provides an empty ObjectId (all zeros).
        /// </summary>
        public static ObjectId Empty
        {
            get { return new ObjectId(); }
        }

        /// <summary>
        /// A 12-byte unique identifier.
        /// </summary>
        public byte[] Value { get; set; }

        /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns></returns>
        public static ObjectId NewObjectId()
        {
            //TODO: generate random-ish bits.
            return new ObjectId {Value = ObjectIdGenerator.Generate()};            
        }

        public static bool TryParse(string value, out ObjectId id)
        {
            id = Empty;
            if (value == null || value.Length != 24)
            {
                return false;
            }
            try
            {
                id = new ObjectId(value);
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
        public override string ToString()
        {
            return BitConverter.ToString(Value).Replace("-", "").ToLower();
        }
    }
}