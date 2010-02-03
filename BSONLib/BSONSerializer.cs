using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.Linq;

namespace BSONLib
{
    /// <summary>
    /// A class that is capable serializing simple .net objects to/from BSON.
    /// </summary>
    public static class BSONSerializer
    {
        private const int CODE_LENGTH = 1;
        private const int KEY_TERMINATOR_LENGTH = 1;

        static BSONSerializer()
        {
            BSONSerializer.Load();
        }

        /// <summary>
        /// Converts a document into its BSON byte-form.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="NotSupportedException">Throws a not supported exception 
        /// when the T is not a "serializable" type.</exception>
        /// <param name="document"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T document, IDictionary<String, object> addedProps)
        {
            if (!BSONSerializer.CanBeSerialized(typeof(T)) || addedProps.Any(y => y.Value != null &&
                !BSONSerializer.CanBeSerialized(y.Value.GetType())))
            {
                throw new NotSupportedException("This type cannot be serialized using the BSONSerializer");
            }
            var getters = BSONSerializer.PropertyInfoFor(document);
            List<byte[]> retval = new List<byte[]>();
            retval.Add(new byte[4]);//allocate size.

            foreach (var member in getters)
            {
                retval.Add(BSONSerializer.SerializeMember(member.Value.Name,
                        member.Value.GetValue(document, null)));
            }

            foreach (var p in addedProps)
            {
                retval.Add(BSONSerializer.SerializeMember(p.Key, p.Value));
            }

            retval.Add(new byte[1]);//null terminate the retval;

            var size = retval.Sum(y => y.Length);
            retval[0] = BitConverter.GetBytes(size);

            return retval.SelectMany(y => y).ToArray();
        }

        private static byte[] SerializeMember(string key, object value)
        {
            //type + name + data
            List<byte[]> retval = new List<byte[]>(4);
            for (int i = 0; i < 4; i++)
            {
                retval.Add(new byte[0]);
            }
            retval[0] = new byte[] { (byte)BSONTypes.Null };
            retval[1] = Encoding.UTF8.GetBytes(key); //push the key into the retval;
            retval[2] = new byte[1];//allocate a null between the key and the value.

            retval[3] = new byte[0];
            //this is where the magic occurs.
            if (value == null)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Null };
                retval[3] = new byte[0];
            }
            else if (value is int?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Int32 };
                retval[3] = BitConverter.GetBytes(((int?)value).Value);
            }
            else if (value is double?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Double };
                retval[3] = BitConverter.GetBytes((double)value);
            }
            else if (value is String)
            {
                retval[0] = new byte[] { (byte)BSONTypes.String };
                //get bytes and append a null to the end.

                var bytes = Encoding.UTF8.GetBytes((String)value)
                    .Concat(new byte[1]).ToArray();
                retval[3] = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
            }
            else if (value is Regex)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Regex };
                var rex = (Regex)value;
                var pattern = rex.ToString();
                String options = "";
                //compiled option can't be set on deserialized regex, therefore - no good can come of this.
                if (rex.Options == RegexOptions.ECMAScript) options += "e";
                if (rex.Options == RegexOptions.IgnoreCase) options += "i";
                if (rex.Options == RegexOptions.CultureInvariant) options += "l";
                if (rex.Options == RegexOptions.Multiline) options += "m";
                if (rex.Options == RegexOptions.Singleline) options += "s";
                //all .net regex are unicode regex, therefore:
                options += "u";
                if (rex.Options == RegexOptions.IgnorePatternWhitespace) options += "w";
                if (rex.Options == RegexOptions.ExplicitCapture) options += "x";

                retval[3] = Encoding.UTF8.GetBytes(pattern).Concat(new byte[1])
                    .Concat(Encoding.UTF8.GetBytes(options)).Concat(new byte[1]).ToArray();
            }
            else if (value is bool?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Boolean };
                retval[3] = BitConverter.GetBytes((bool)value);
            }
            else if (value is byte[])
            {
                retval[0] = new byte[] { (byte)BSONTypes.Binary };
                var binary = new List<byte[]>();
                binary.Add(new byte[0]);//do NOT allocate space for the size -- this is different than most BSON cases.
                binary.Add(new byte[] { (byte)2 });//describe the binary
                var theBytes = (byte[])value;
                binary.Add(BitConverter.GetBytes(theBytes.Length));//describe the number of bytes.
                binary.Add(theBytes);//add the bytes
                binary[0] = BitConverter.GetBytes(binary.Sum(y => y.Length) - 1);//set the total binary size (after the subtype.. weird)
                //not sure if this is correct.
                retval[3] = binary.SelectMany(h => h).ToArray();
            }
            else if (value is Guid?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Binary };
                var binary = new List<byte[]>();
                binary.Add(BitConverter.GetBytes(16));
                Guid? val = (Guid?)value;
                binary.Add(new byte[] { (byte)3 });
                binary.Add(val.Value.ToByteArray());
                retval[3] = binary.SelectMany(y => y).ToArray();
            }
            else if (value is BSONOID)
            {
                retval[0] = new byte[] { (byte)BSONTypes.MongoOID };
                var oid = (BSONOID)value;
                retval[3] = oid.Value;
            }
            else if (value is BSONReference)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Reference };
                //TODO: serialize document reference.
            }
            else if (value is BSONScopedCode)
            {
                retval[0] = new byte[] { (byte)BSONTypes.ScopedCode };
                //TODO: serialize scoped code.
            }
            else if (value is DateTime)
            {
                retval[0] = new byte[] { (byte)BSONTypes.DateTime };
                //TODO: serialize date time
            }
            else if (value is long?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Int64 };
                retval[3] = BitConverter.GetBytes((long)value);
            }
            //TODO: implement something for "Symbol"
            //TODO: implement non-scoped code handling.
            else
            {
                retval[0] = new byte[] { (byte)BSONTypes.Object };
                retval[3] = BSONSerializer.Serialize(value);
            }

            return retval.SelectMany(h => h).ToArray();
        }

        /// <summary>
        /// Converts a document's byte-form back into a POCO.
        /// </summary>
        /// <typeparam name="T">The type to be converted back to.</typeparam>
        /// <param name="stream">the document's bytes</param>
        /// <param name="outProps">Properties that don't map onto T.</param>
        /// <returns></returns>
        public static T Deserialize<T>(BinaryReader stream, out IDictionary<String, object> outProps) where T : class, new()
        {
            if (!BSONSerializer.CanBeDeserialized(typeof(T)))
            {
                throw new NotSupportedException("This type cannot be serialized using the BSONSerializer");
            }

            int length = stream.ReadInt32();
            //get the object length minus the header and the null (5)
            byte[] buffer = new byte[length - 5];
            stream.Read(buffer, 0, length - 5);
            //push the position forward past the null terminator.
            stream.Read(new byte[1], 0, 1);
            T retval = new T();
            var setters = BSONSerializer.PropertyInfoFor(retval);
            outProps = new Dictionary<String, object>(0);

            while (buffer.Length > 0)
            {
                BSONTypes t = (BSONTypes)buffer[0];
                var stringBytes = buffer.Skip(1)
                    .TakeWhile(y => y != (byte)0)
                    .ToArray();

                String key = Encoding.UTF8.GetString(stringBytes);
                //the object data is everything other than the key and the null.
                var objectData = buffer.Skip(stringBytes.Length + 2).ToArray();


                int usedBytes;
                var obj = BSONSerializer.DeserializeMember(t, objectData, out usedBytes);

                //skip type, the key, the null, the object data
                buffer = buffer.Skip(CODE_LENGTH + stringBytes.Length +
                    KEY_TERMINATOR_LENGTH + usedBytes).ToArray();

                if (setters.ContainsKey(key.ToLower()))
                {
                    var prop = setters[key.ToLower()];
                    prop.SetValue(retval, obj, null);
                }
                else
                {
                    outProps[key] = obj;
                }
            }

            return retval;
        }

        public static byte[] Serialize<T>(T objectToSerialize)
        {
            return BSONSerializer.Serialize<T>(objectToSerialize, new Dictionary<String, object>(0));
        }




        /// <summary>
        /// Overload that constructs a BinaryReader in memory and then deserializes the values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectData"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData, out IDictionary<String, object> outProps) where T : class, new()
        {
            var ms = new MemoryStream();
            ms.Write(objectData, 0, objectData.Length);
            ms.Position = 0;
            return BSONSerializer.Deserialize<T>(new BinaryReader(ms), out outProps);
        }

        public static T Deserialize<T>(byte[] objectData) where T : class, new()
        {
            IDictionary<String, object> outprops;
            return BSONSerializer.Deserialize<T>(objectData, out outprops);
        }

        private static object DeserializeMember(BSONTypes t, byte[] objectData, out int usedBytes)
        {
            object retval = null;
            usedBytes = 0;
            //type + name + data
            if (t == BSONTypes.Null)
            {
                retval = null;
                usedBytes = 0;
            }
            if (t == BSONTypes.Int32)
            {
                retval = BitConverter.ToInt32(objectData, 0);
                usedBytes = 4;
            }
            else if (t == BSONTypes.Double)
            {
                retval = BitConverter.ToDouble(objectData, 0);
                usedBytes = 8;
                //handle FLOAT.

            }
            else if (t == BSONTypes.String)
            {
                var stringLength = BitConverter.ToInt32(objectData, 0) - 1;//remove the null.
                var stringData = objectData.Skip(4).Take(stringLength).ToArray();
                retval = Encoding.UTF8.GetString(stringData);
                usedBytes = stringLength + 5;//account for size and null

            }
            else if (t == BSONTypes.Regex)
            {

                var patternBytes = objectData.TakeWhile(y => y != 0).ToArray();
                var optionBytes = objectData.Skip(patternBytes.Length + 1).TakeWhile(y => y != 0).ToArray();

                String optionString = Encoding.UTF8.GetString(optionBytes);
                String pattern = Encoding.UTF8.GetString(patternBytes);

                RegexOptions options = RegexOptions.None;
                if (optionString.Contains("e")) options = options == RegexOptions.None ? RegexOptions.ECMAScript : (options | RegexOptions.ECMAScript);
                if (optionString.Contains("i")) options = options == RegexOptions.None ? RegexOptions.IgnoreCase : (options | RegexOptions.IgnoreCase);
                if (optionString.Contains("l")) options = options == RegexOptions.None ? RegexOptions.CultureInvariant : (options | RegexOptions.CultureInvariant);
                if (optionString.Contains("m")) options = options == RegexOptions.None ? RegexOptions.Multiline : (options | RegexOptions.Multiline);
                if (optionString.Contains("s")) options = options == RegexOptions.None ? RegexOptions.Singleline : (options | RegexOptions.Singleline);
                if (optionString.Contains("w")) options = options == RegexOptions.None ? RegexOptions.IgnorePatternWhitespace : (options | RegexOptions.IgnorePatternWhitespace);
                if (optionString.Contains("x")) options = options == RegexOptions.None ? RegexOptions.ExplicitCapture : (options | RegexOptions.ExplicitCapture);

                usedBytes = patternBytes.Length + optionBytes.Length + 2;
                retval = new Regex(pattern, options);
            }

            else if (t == BSONTypes.Boolean)
            {
                retval = BitConverter.ToBoolean(objectData, 0);
                usedBytes = 1;
            }
            else if (t == BSONTypes.Binary)
            {
                var length = BitConverter.ToInt32(objectData, 0);
                var binaryType = (int)objectData[4];
                if (binaryType == 2)
                {
                    var binaryLength = BitConverter.ToInt32(objectData, 5);
                    retval = objectData.Skip(9).Take(binaryLength).ToArray();
                    usedBytes = binaryLength + 9;
                }
                else if (binaryType == 3)
                {
                    retval = new Guid(objectData.Skip(5).Take(16).ToArray());
                    usedBytes = 21;
                }
            }
            else if (t == BSONTypes.MongoOID)
            {
                retval = new BSONOID() { Value = objectData };
                usedBytes = 12;
            }
            else if (t == BSONTypes.Reference)
            {
                //TODO: deserialize document reference.
            }
            else if (t == BSONTypes.ScopedCode)
            {
                //TODO: deserialize scoped code.
            }
            else if (t == BSONTypes.DateTime)
            {
                //TODO: deserialize date time
            }
            else if (t == BSONTypes.Int64)
            {
                retval = BitConverter.ToInt64(objectData, 0);
                usedBytes = 8;
            }
            //TODO: implement something for "Symbol"
            //TODO: implement non-scoped code handling.
            else
            {
                //Object deserialization needs to be handled a level up.   
            }
            return retval;
        }

        /// <summary>
        /// Types that can be serialized to/from BSON.
        /// </summary>
        private static HashSet<Type> _allowedTypes = new HashSet<Type>();

        /// <summary>
        /// The types that can be deserialized.
        /// </summary>
        private static HashSet<Type> _canDeserialize = new HashSet<Type>();

        /// <summary>
        /// Types that cannot be serialized to/from BSON.
        /// </summary>
        private static HashSet<Type> _prohibittedTypes = new HashSet<Type>();

        /// <summary>
        /// delegates to setters for specific types.
        /// </summary>
        private static Dictionary<Type, Dictionary<String, PropertyInfo>> _setters =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        /// <summary>
        /// Sets some "white-listed" types that the BSONSerializer knows about.
        /// </summary>
        private static void Load()
        {
            // whitelist a few "complex" types (reference types that have 
            // additional properties that will not be serialized)
            if (BSONSerializer._allowedTypes.Count == 0)
            {
                //these are all known "safe" types that the reader handles.
                BSONSerializer._allowedTypes.Add(typeof(int?));
                BSONSerializer._allowedTypes.Add(typeof(long?));
                BSONSerializer._allowedTypes.Add(typeof(bool?));
                BSONSerializer._allowedTypes.Add(typeof(double?));
                BSONSerializer._allowedTypes.Add(typeof(Guid?));
                BSONSerializer._allowedTypes.Add(typeof(DateTime?));
                BSONSerializer._allowedTypes.Add(typeof(String));
                BSONSerializer._allowedTypes.Add(typeof(BSONOID));
                BSONSerializer._allowedTypes.Add(typeof(Regex));
                BSONSerializer._allowedTypes.Add(typeof(byte[]));
                BSONSerializer._allowedTypes.Add(typeof(Regex));
                foreach (var t in BSONSerializer._allowedTypes)
                {
                    BSONSerializer._canDeserialize.Add(t);
                }

                //these can be serialized, but not deserialized.
                BSONSerializer._allowedTypes.Add(typeof(int));
                BSONSerializer._allowedTypes.Add(typeof(double));
                BSONSerializer._allowedTypes.Add(typeof(long));
                BSONSerializer._allowedTypes.Add(typeof(bool));
                BSONSerializer._allowedTypes.Add(typeof(DateTime));
                BSONSerializer._allowedTypes.Add(typeof(Guid));
            }
        }

        /// <summary>
        /// Will traverse the class definition for public instance properties and
        /// determine if their types are serializeable.
        /// </summary>
        /// <remarks>
        /// Only R/W public properties may exist in the specified type. The type must be a class, or
        /// one of the "whitelisted types" (Oid, MongoRegex, Binary, String, double?,int?,long?, DateTime?, bool?)
        /// Property types must also follow these constraints. (meaning that "int" is not supported but "int?" is.)
        /// 
        /// 'Why?' you ask. Allowing structs that can have default values will lead to confusion and bugs when
        /// people inevitably assume that a property value of 0 was the one set by the database. no, int? is better.
        /// </remarks>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CanBeSerialized(Type t)
        {
            bool retval = true;
            //we want to check to see if this type can be serialized.
            if (!BSONSerializer._prohibittedTypes.Contains(t) &&
                !BSONSerializer._allowedTypes.Contains(t))
            {
                ///we only care about public properties on instances, not statics.
                foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    retval &= pi.CanRead;
                    var propType = pi.PropertyType;
                    if (!propType.IsValueType)
                    {
                        retval &= BSONSerializer.CanBeSerialized(propType);
                    }
                }
            }
            else if (BSONSerializer._prohibittedTypes.Contains(t))
            {
                retval = false;
            }

            //if we get all the way to the end, this type is "safe" and we should include actions.
            if (retval == true)
            {
                BSONSerializer._allowedTypes.Add(t);
            }
            return retval;

        }


        /// <summary>
        /// Indicates that a type can be hydrated by the BSONSerializer
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CanBeDeserialized(Type t)
        {
            bool retval = true;
            //we want to check to see if this type can be serialized.
            if (!BSONSerializer._canDeserialize.Contains(t) &&
                !BSONSerializer._prohibittedTypes.Contains(t))
            {
                ///we only care about public properties on instances, not statics.
                foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    retval &= pi.CanWrite & pi.CanWrite;
                    var propType = pi.PropertyType;
                    if (!propType.IsValueType)
                    {
                        retval &= BSONSerializer.CanBeDeserialized(propType);
                    }
                }
            }
            else if (BSONSerializer._prohibittedTypes.Contains(t))
            {
                retval = false;
            }

            //if we get all the way to the end, this type is "safe" and we should include actions.
            if (retval == true && !BSONSerializer._canDeserialize.Contains(t))
            {
                BSONSerializer._canDeserialize.Add(t);
                BSONSerializer._allowedTypes.Add(t);
            }
            return retval;
        }

        /// <summary>
        /// Key: Lowercase name of the property
        /// Value: MethodInfo to be used to call said property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        private static IDictionary<String, PropertyInfo> PropertyInfoFor<T>(T document)
        {
            if (!BSONSerializer._setters.ContainsKey(typeof(T)))
            {
                BSONSerializer._setters[typeof(T)] = new Dictionary<string, PropertyInfo>();
                foreach (var p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    BSONSerializer._setters[typeof(T)][p.Name.ToLower()] = p;
                }
            }
            return new Dictionary<String, PropertyInfo>(BSONSerializer._setters[typeof(T)]);
        }
    }
}