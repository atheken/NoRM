namespace NoRM
{
    using System;
    using BSON.DbTypes;

    public class ObjectId
    {
        private byte[] _value;
        private string _string;

        public byte[] Value
        {
            get { return _value; }
        }

        public ObjectId(){}
        public ObjectId(string value) : this(DecodeHex(value)){}        
        internal ObjectId(byte[] value)
        {
            _value = value;
        }

        /// <summary>
        /// Provides an empty ObjectId (all zeros).
        /// </summary>
        public static ObjectId Empty
        {
            get { return new ObjectId("000000000000000000000000"); }
        }

 /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns></returns>
        public static ObjectId NewObjectId()
        {
            //TODO: generate random-ish bits.
            return new ObjectId { _value = ObjectIdGenerator.Generate() };            
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
            if (_string == null && _value != null)
            {
                _string = BitConverter.ToString(_value).Replace("-", string.Empty).ToLower();
            }
            return _string;
        }

        public override bool Equals(object o)
        {
            var other = o as ObjectId;
            return other != null && ToString() == other.ToString();
        }

        public override int GetHashCode()
        {
            return (_value != null ? ToString().GetHashCode() : 0);
        }
    }
}