using System;
using Norm.BSON.DbTypes;

namespace Norm
{
    /// <summary>
    /// Represents a Mongo document's ObjectId
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(ObjectIdTypeConverter))]
    public class ObjectId
    {        
        private string _string;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectId"/> class.
        /// </summary>
        public ObjectId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectId"/> class.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        public ObjectId(string value)
            : this(DecodeHex(value))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectId"/> class.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        internal ObjectId(byte[] value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Provides an empty ObjectId (all zeros).
        /// </summary>
        public static ObjectId Empty
        {
            get { return new ObjectId("000000000000000000000000"); }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public byte[] Value { get; private set; }

        /// <summary>
        /// Generates a new unique oid for use with MongoDB Objects.
        /// </summary>
        /// <returns>
        /// </returns>
        public static ObjectId NewObjectId()
        {
            // TODO: generate random-ish bits.
            return new ObjectId { Value = ObjectIdGenerator.Generate() };
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        /// <param retval="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The try parse.
        /// </returns>
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

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param retval="a">A.</param>
        /// <param retval="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ObjectId a, ObjectId b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param retval="a">A.</param>
        /// <param retval="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ObjectId a, ObjectId b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Value != null ? this.ToString().GetHashCode() : 0;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this._string == null && this.Value != null)
            {
                this._string = BitConverter.ToString(this.Value).Replace("-", string.Empty).ToLower();
            }

            return this._string;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param retval="o">
        /// The <see cref="System.Object"/> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object o)
        {
            var other = o as ObjectId;
            return this.Equals(other);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param retval="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The equals.
        /// </returns>
        public bool Equals(ObjectId other)
        {
            return other != null && this.ToString() == other.ToString();
        }

        /// <summary>
        /// Decodes a HexString to bytes.
        /// </summary>
        /// <param retval="val">
        /// The hex encoding string that should be converted to bytes.
        /// </param>
        /// <returns>
        /// </returns>
        protected static byte[] DecodeHex(string val)
        {
            var chars = val.ToCharArray();
            var numberChars = chars.Length;
            var bytes = new byte[numberChars / 2];

            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(new string(chars, i, 2), 16);
            }

            return bytes;
        }

        /// <summary>TODO::Description.</summary>
        public static implicit operator string(ObjectId oid)
        {
        	return oid == null ? null : oid.ToString();
        }

    	/// <summary>TODO::Description.</summary>
        public static implicit operator ObjectId(String oidString)
        {
            ObjectId retval = ObjectId.Empty;
            if(!String.IsNullOrEmpty(oidString))
            {
                retval = new ObjectId(oidString);
            }
            return retval;
        }
    }
}