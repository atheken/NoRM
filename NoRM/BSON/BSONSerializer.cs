using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.Linq;
using System.Collections;
using NoRM.BSON.DbTypes;
using NoRM.Attributes;

namespace NoRM.BSON
{
    /// <summary>
    /// A class that is capable serializing simple .net objects to/from BSON.
    /// </summary>
    /// <remarks>
    /// In here there be dragons. Proceed at your own risk.
    /// </remarks>
    public static class BSONSerializer
    {
        #region Reflection Cache
        /// <summary>
        /// Types that can be serialized to/from BSON.
        /// </summary>
        private static HashSet<Type> _allowedTypes = new HashSet<Type>();

        /// <summary>
        /// The delegates to getters for specific types.
        /// </summary>
        private static Dictionary<Type, Dictionary<String, Func<object, object>>> _getters = new Dictionary<Type, Dictionary<string, Func<object, object>>>();


        /// <summary>
        /// Sets some "white-listed" types that the BSONSerializer knows about.
        /// </summary>
        private static void Load()
        {
            // whitelist a few "complex" types (reference types that have 
            // additional properties that will not be serialized)
            if (_allowedTypes.Count == 0)
            {
                //these are all known "safe" types that the reader handles.
                _allowedTypes.Add(typeof(int?));
                _allowedTypes.Add(typeof(long?));
                _allowedTypes.Add(typeof(bool?));
                _allowedTypes.Add(typeof(double?));
                _allowedTypes.Add(typeof(Guid?));
                _allowedTypes.Add(typeof(DateTime?));
                _allowedTypes.Add(typeof(String));
                _allowedTypes.Add(typeof(ObjectId));
                _allowedTypes.Add(typeof(Regex));
                _allowedTypes.Add(typeof(byte[]));
                _allowedTypes.Add(typeof(Regex));
                _allowedTypes.Add(typeof(IList));
   
                //these can be serialized, but not deserialized.
                _allowedTypes.Add(typeof(Enum));
                _allowedTypes.Add(typeof(int));
                _allowedTypes.Add(typeof(double));
                _allowedTypes.Add(typeof(long));
                _allowedTypes.Add(typeof(bool));
                _allowedTypes.Add(typeof(DateTime));
                _allowedTypes.Add(typeof(Guid));
            }
        }

        #endregion

        #region Reflection Helpers
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
            return BSONSerializer.CanBeSerialized(t, new HashSet<Type>());
        }

        /// <summary>
        /// This gets the name for the property on the mongo document.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="CLRName"></param>
        /// <returns></returns>
        public static String GetMongoNameForPropertyName(this String CLRName, Type className)
        {
            return CLRName;
        }

        /// <summary>
        /// This gets the name for the property on the object.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="MongoName"></param>
        /// <returns></returns>
        public static String GetCLRNameForMongoName(this String MongoName, Type className)
        {
            return MongoName;
        }

        /// <summary>
        /// This is a helper method for the public CanBeSerialized that avoids infinite 
        /// recursion by tracking which types are already being checked and not checking them.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="checkStack"></param>
        /// <returns></returns>
        private static bool CanBeSerialized(Type t, HashSet<Type> checkStack)
        {
            bool retval = true;
            //we want to check to see if this type can be serialized.

            if (!BsonHelper.ProhibittedTypes.Contains(t) && !_allowedTypes.Contains(t))
            {
                if (typeof(Enum).IsAssignableFrom(t))
                {
                    retval = true;
                }
                else
                {
                    ///we only care about public properties on instances, not statics.
                    foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        retval &= pi.CanRead;
                        var propType = pi.PropertyType;

                        //only do a check on a particular type once.
                        if (!checkStack.Contains(propType))
                        {
                            checkStack.Add(propType);
                            if (!propType.IsValueType)
                            {
                                retval &= BSONSerializer.CanBeSerialized(propType, checkStack);
                            }
                        }
                    }
                }
            }
            else if (BsonHelper.ProhibittedTypes.Contains(t))
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
        /// Key: Lowercase name of the property
        /// Value: A delegate to set the value on the target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        private static IDictionary<String, Func<object, object>> GettersForType(Type documentType)
        {
            if (!BSONSerializer._getters.ContainsKey(documentType))
            {
                BSONSerializer._getters[documentType] = new Dictionary<string, Func<object, object>>
                    (StringComparer.InvariantCultureIgnoreCase);

                foreach (var p in documentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(y => y.GetIndexParameters().Count() == 0 &&!y.GetCustomAttributes(true).Any(f => f is MongoIgnoreAttribute)).OrderBy(y => y.GetCustomAttributes(true).Any(x => x is MongoIdentifierAttribute) ? 3 : y.Name == "_id" ? 2 : string.Compare(y.Name, "ID", true) == 0 ? 1 : 0))
                {
                    BSONSerializer._getters[documentType][p.Name] = ReflectionHelpers.GetterMethod(p);
                }
            }
            return new Dictionary<String, Func<object, object>>(BSONSerializer._getters[documentType],
                StringComparer.InvariantCultureIgnoreCase);
        }
        #endregion

        static BSONSerializer()
        {
            BSONSerializer.Load();
        }

        #region Serialization

        /// <summary>
        /// Converts a document into its BSON byte-form.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="NotSupportedException">Throws a not supported exception 
        /// when the T is not a "serializable" type.</exception>
        /// <param name="document"></param>
        /// <param name="includeExpandoProps">true indicates that Flyweight associated with this object
        /// should be included in serialization. 
        /// False means that it will be ignored.</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T document, bool includeExpandoProps)
        {

            Flyweight props = null;
            if (includeExpandoProps)
            {
                props = ExpandoProps.FlyweightForObject(document);
            }
            if (!BSONSerializer.CanBeSerialized(typeof(T)) ||
                (props != null && props.AllProperties().Any(y => y.Value != null &&
                !BSONSerializer.CanBeSerialized(y.Value.GetType()))))
            {
                throw new NotSupportedException("This type cannot be SERIALIZED using the BSONSerializer");
            }

            List<byte[]> retval = new List<byte[]>();

            Dictionary<String, object> values = null;

            if (document is Flyweight)
            {
                var flyweight = (document as Flyweight);
                values = flyweight.AllProperties().ToDictionary(y => y.PropertyName, k => k.Value);
            }
            else
            {
                values = BSONSerializer.GettersForType(document.GetType())
                    .ToDictionary(y => y.Key, h => h.Value.Invoke(document));
                if (props != null && includeExpandoProps)
                {
                    foreach (var p in props.AllProperties())
                    {
                        values.Add(p.PropertyName, p.Value);
                    }
                }
            }
            retval.Add(new byte[4]);//allocate size.
            foreach (var member in values)
            {

                var obj = member.Value;
                var name = member.Key;

                //"special" case.
                if (obj is ModifierCommand)
                {
                    var o = obj as ModifierCommand;
                    //set type of member.
                    retval.Add(new byte[] { (byte)BSONTypes.Object });
                    //set name of member
                    retval.Add(o.CommandName.CStringBytes());

                    //construct member bytes
                    var modValue = new List<byte[]>();
                    modValue.Add(new byte[4]);//allocate size.
                    modValue.Add(BSONSerializer.SerializeMember(name, o.ValueForCommand));//then serialize the member.
                    modValue.Add(new byte[1]);//null terminate this member.
                    modValue[0] = BitConverter.GetBytes(modValue.Sum(y => y.Length));

                    //add this to the main retval.
                    retval.AddRange(modValue);

                }
                else if (obj is QualifierCommand)
                {
                    //wow, this is insane, the idiom for "query" commands is exactly opposite of that for "update" commands.
                    var o = obj as QualifierCommand;
                    //set type of member.
                    retval.Add(new byte[] { (byte)BSONTypes.Object });
                    //set name of member
                    retval.Add(name.CStringBytes());

                    //construct member bytes
                    var modValue = new List<byte[]>();
                    modValue.Add(new byte[4]);//allocate size.
                    modValue.Add(BSONSerializer.SerializeMember(o.CommandName, o.ValueForCommand));//then serialize the member.
                    modValue.Add(new byte[1]);//null terminate this member.
                    modValue[0] = BitConverter.GetBytes(modValue.Sum(y => y.Length));//add this to the main retval.

                    retval.AddRange(modValue);
                }
                else
                {
                    retval.Add(BSONSerializer.SerializeMember(name, obj));
                }
            }


            retval.Add(new byte[1]);//null terminate the retval;

            var size = retval.Sum(y => y.Length);
            retval[0] = BitConverter.GetBytes(size);

            return retval.SelectMany(y => y).ToArray();
        }

        /// <summary>
        /// Serialize an object to bytes, ignoring the expando properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T objectToSerialize)
        {
            return BSONSerializer.Serialize<T>(objectToSerialize, false);
        }

        /// <summary>
        /// Convert an object to a byte array, this should really be broken out into
        /// separate methods so that we don't have to do so much casting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] SerializeMember(String key, object value)
        {
            var isEnum = (value is Enum);
            Type enumType = null;
            if (isEnum)
            {
                enumType = Enum.GetUnderlyingType(value.GetType());
            }

            //type + name + data
            List<byte[]> retval = new List<byte[]>(4);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);


            retval[0] = new byte[] { (byte)BSONTypes.Null };
            retval[1] = string.Compare(key, "id", true) == 0 ? "_id".CStringBytes() : key.CStringBytes(); //todo: fix
            retval[2] = new byte[0];
            //this is where the magic occurs
            //HEED MY WARNING! ALWAYS test for NULL first, as other things below ASSUME that if it gets past here, it's NOT NULL!
            if (value == null)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Null };
                retval[2] = new byte[0];
            }
            else if (value is int || (isEnum && typeof(int).IsAssignableFrom(enumType)))
            {
                retval[0] = new byte[] { (byte)BSONTypes.Int32 };
                retval[2] = BitConverter.GetBytes((int)value);
            }
            else if (value is double || (isEnum && typeof(double).IsAssignableFrom(enumType)))
            {
                retval[0] = new byte[] { (byte)BSONTypes.Double };
                retval[2] = BitConverter.GetBytes((double)value);
            }
            else if (value is String)
            {
                retval[0] = new byte[] { (byte)BSONTypes.String };
                //get bytes and append a null to the end.

                var bytes = ((String)value).CStringBytes();
                retval[2] = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
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

                retval[2] = Encoding.UTF8.GetBytes(pattern).Concat(new byte[1])
                    .Concat(Encoding.UTF8.GetBytes(options)).Concat(new byte[1]).ToArray();
            }
            else if (value is bool)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Boolean };
                retval[2] = BitConverter.GetBytes((bool)value);
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
                retval[2] = binary.SelectMany(h => h).ToArray();
            }
            else if (value is Guid)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Binary };
                var binary = new List<byte[]>();
                binary.Add(BitConverter.GetBytes(16));
                Guid? val = (Guid?)value;
                binary.Add(new byte[] { (byte)3 });
                binary.Add(val.Value.ToByteArray());
                retval[2] = binary.SelectMany(y => y).ToArray();
            }
            else if (value is ObjectId)
            {
                retval[0] = new byte[] { (byte)BSONTypes.MongoOID };
                var oid = (ObjectId)value;
                retval[2] = oid.Value;
            }
            else if (value is DBReference)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Reference };
                //TODO: serialize document reference.
            }
            else if (value is ScopedCode)
            {
                retval[0] = new byte[] { (byte)BSONTypes.ScopedCode };
                ScopedCode code = value as ScopedCode;
                List<byte[]> scopedCode = new List<byte[]>();
                scopedCode.Add(new byte[4]);
                scopedCode.Add(new byte[4]);
                scopedCode.Add((code.CodeString ?? "").CStringBytes());
                scopedCode.Add(BSONSerializer.Serialize(code.Scope));
                scopedCode.Add(new byte[1]);
                scopedCode[0] = BitConverter.GetBytes(scopedCode.Sum(y => y.Length));
                scopedCode[1] = BitConverter.GetBytes(scopedCode[2].Length);
                retval[2] = scopedCode.SelectMany(y => y).ToArray();
            }
            else if (value is DateTime?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.DateTime };
                retval[2] = BitConverter.GetBytes((long)(((DateTime?)value).Value - BsonHelper.EPOCH).TotalMilliseconds);
            }
            else if (value is long || (isEnum && typeof(long).IsAssignableFrom(enumType)))
            {
                retval[0] = new byte[] { (byte)BSONTypes.Int64 };
                retval[2] = BitConverter.GetBytes((long)value);
            }
            //handle arrays and the like.
            else if (value is IEnumerable)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Array };
                int index = -1;
                var o = value as IEnumerable;
                var memberBytes = o.OfType<Object>().Select(y =>
                {
                    index++;
                    return BSONSerializer.SerializeMember(index.ToString(), y);
                }).SelectMany(h => h).ToArray();

                retval[2] = BitConverter.GetBytes(4 + memberBytes.Length + 1)
                    .Concat(memberBytes).Concat(new byte[1]).ToArray();
            }
            //TODO: implement something for "Symbol"
            //TODO: implement non-scoped code handling.
            else
            {
                retval[0] = new byte[] { (byte)BSONTypes.Object };
                retval[2] = BSONSerializer.Serialize(value);
            }

            return retval.SelectMany(h => h).ToArray();
        }

        /// <summary>
        /// Convert an object to a byte array, this should really be broken out into
        /// separate methods so that we don't have to do so much casting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] SerializeMemberFast(String key, object value)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            WriteBSONType(value, binaryWriter);
            binaryWriter.Write(Encoding.UTF8.GetBytes(key));
            binaryWriter.Write(new byte[1]);
            WriteBSONValue(value, binaryWriter);
           
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Convert an object to a byte array, this should really be broken out into
        /// separate methods so that we don't have to do so much casting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static void SerializeMemberFast(String key, object value, BinaryWriter binaryWriter)
        {
            WriteBSONType(value, binaryWriter);
            binaryWriter.Write(key);
            binaryWriter.Write(new byte[1]);
            WriteBSONValue(value, binaryWriter);
        }

        private static void WriteBSONType<T>(T value, BinaryWriter writer)
        {
            //this is where the magic occurs.
            if (value == null)
            {
                writer.Write((byte) BSONTypes.Null);
                return;
            }
            else if (value is int?)
            {
                writer.Write((byte)BSONTypes.Int32);
                return;
            }
            else if (value is double?)
            {
                writer.Write((byte)BSONTypes.Double);
                return;
            }
            else if (value is String)
            {
               writer.Write((byte)BSONTypes.String);
               return;
            }
            else if (value is Regex)
            {
                writer.Write((byte) BSONTypes.Regex);
                return;
            }
            else if (value is bool?)
            {
                writer.Write((byte) BSONTypes.Boolean);
                return;
            }
            else if (value is byte[])
            {
                writer.Write((byte)BSONTypes.Binary);
                return;
            }
            else if (value is Guid?)
            {
                writer.Write((byte)BSONTypes.Binary);
                return;
            }
            else if (value is ObjectId)
            {
               writer.Write((byte)BSONTypes.MongoOID);
               return;
            }
            else if (value is DBReference)
            {
                writer.Write((byte) BSONTypes.Reference);
                return;
            }
            else if (value is ScopedCode)
            {
                writer.Write((byte) BSONTypes.ScopedCode);
                return;
            }
            else if (value is DateTime?)
            {
                writer.Write((byte) BSONTypes.DateTime);
                return;
            }
            else if (value is long?)
            {
                writer.Write((byte) BSONTypes.Int64);
                return;
            }
            //handle arrays and the like.
            else if (value is IEnumerable)
            {
                writer.Write((byte) BSONTypes.Array);
                return;
            }
            //TODO: implement something for "Symbol"
            //TODO: implement non-scoped code handling.

            writer.Write((byte)BSONTypes.Object);
        }

        private static void WriteBSONValue(object value, BinaryWriter writer)
        {
            //this is where the magic occurs.
            if (value == null)
            {
                writer.Write( new byte[0]);
                return;
            }
            else if (value is int?)
            {
                writer.Write( ((int?)value).Value);
                return;
            }
            else if (value is double?)
            {

                writer.Write((double)value);
                return;
            }
            else if (value is String)
            {
                ((String)value).CStringBytes(writer);
                return;
            }
            else if (value is Regex)
            {
                var rex = (Regex)value;
                var pattern = rex.ToString();
                var options = "";
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

                writer.Write(Encoding.UTF8.GetBytes(pattern).Concat(new byte[1]).ToArray());
                writer.Write(Encoding.UTF8.GetBytes(options).Concat(new byte[1]).ToArray());
                return;
            }
            else if (value is bool?)
            {
                writer.Write((bool)value);
                return;
            }
            else if (value is byte[])
            {
                //change to write directly to weiter
                var binary = new List<byte[]>();
                binary.Add(new byte[0]);//do NOT allocate space for the size -- this is different than most BSON cases.
                binary.Add(new byte[] { (byte)2 });//describe the binary
                var theBytes = (byte[])value;
                binary.Add(BitConverter.GetBytes(theBytes.Length));//describe the number of bytes.
                binary.Add(theBytes);//add the bytes
                binary[0] = BitConverter.GetBytes(binary.Sum(y => y.Length) - 1);//set the total binary size (after the subtype.. weird)
                //not sure if this is correct.
                writer.Write( binary.SelectMany(h => h).ToArray());
                return;
            }
            else if (value is Guid?)
            {
                var binary = new List<byte[]>();
                binary.Add(BitConverter.GetBytes(16));
                Guid? val = (Guid?)value;
                binary.Add(new byte[] { (byte)3 });
                binary.Add(val.Value.ToByteArray());
                writer.Write( binary.SelectMany(y => y).ToArray());
                return;
            }
            else if (value is ObjectId)
            {
                writer.Write( ((ObjectId) value).Value);
                return;
            }
            else if (value is DBReference)
            {
                //TODO: serialize document reference.
                return;
            }
            else if (value is ScopedCode)
            {
                var code = value as ScopedCode;
                var scopedCode = new List<byte[]>();
                scopedCode.Add(new byte[4]);
                scopedCode.Add(new byte[4]);
                scopedCode.Add((code.CodeString ?? "").CStringBytes());
                scopedCode.Add(BSONSerializer.Serialize(code.Scope));
                scopedCode[0] = BitConverter.GetBytes(scopedCode.Sum(y => y.Length));
                scopedCode[1] = BitConverter.GetBytes(scopedCode[2].Length);

                writer.Write( scopedCode.SelectMany(h => h).ToArray());
                return;
            }
            else if (value is DateTime?)
            {
                writer.Write( BitConverter.GetBytes((long)(((DateTime?)value).Value - BsonHelper.EPOCH).TotalMilliseconds));
                return;
            }
            else if (value is long?)
            {
                writer.Write( BitConverter.GetBytes((long)value));
                return;
            }
            //handle arrays and the like.
            else if (value is IEnumerable)
            {
                int index = -1;
                var o = value as IEnumerable;
                var memberBytes = o.OfType<Object>().Select(y =>
                {
                    index++;
                    return BSONSerializer.SerializeMember(index.ToString(), y);
                }).SelectMany(h => h).ToArray();

                writer.Write( BitConverter.GetBytes(4 + memberBytes.Length + 1)
                    .Concat(memberBytes).Concat(new byte[1]).ToArray());
                return;
            }
            //TODO: implement something for "Symbol"
            //TODO: implement non-scoped code handling.
            
            writer.Write( BSONSerializer.Serialize(value));
        }


        #endregion

        /// <summary>
        /// Encodes a string to UTF-8 bytes and adds a null byte to the end.
        /// </summary>
        /// <param name="stringToEncode"></param>
        /// <returns></returns>
        public static byte[] CStringBytes(this String stringToEncode)
        {
            return Encoding.UTF8.GetBytes(stringToEncode).Concat(new byte[1]).ToArray();
        }

        /// <summary>
        /// Encodes a string to UTF-8 bytes and adds a null byte to the end.
        /// </summary>
        /// <param name="stringToEncode"></param>
        /// <returns></returns>
        public static void CStringBytes(this String stringToEncode, BinaryWriter binaryWriter)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToEncode);
            binaryWriter.Write(bytes.Length+ 1);
            binaryWriter.Write(bytes);
            binaryWriter.Write(new byte[1]);
        }
    }
}