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
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);
        private const int CODE_LENGTH = 1;
        private const int KEY_TERMINATOR_LENGTH = 1;

        #region Reflection Cache
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
        /// A list of the known properties for a type, and their types.
        /// </summary>
        private static Dictionary<Type, Dictionary<String, Type>> _propertyTypes = new Dictionary<Type, Dictionary<string, Type>>();

        /// <summary>
        /// delegates to setters for specific types.
        /// </summary>
        private static Dictionary<Type, Dictionary<String, Action<object, object>>> _setters =
            new Dictionary<Type, Dictionary<string, Action<object, object>>>();

        /// <summary>
        /// The delegates to getters for specific types.
        /// </summary>
        private static Dictionary<Type, Dictionary<String, Func<object, object>>> _getters =
            new Dictionary<Type, Dictionary<string, Func<object, object>>>();


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
                BSONSerializer._allowedTypes.Add(typeof(OID));
                BSONSerializer._allowedTypes.Add(typeof(Regex));
                BSONSerializer._allowedTypes.Add(typeof(byte[]));
                BSONSerializer._allowedTypes.Add(typeof(Regex));
                BSONSerializer._allowedTypes.Add(typeof(IList));
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
            if (!BSONSerializer._prohibittedTypes.Contains(t) &&
                !BSONSerializer._allowedTypes.Contains(t))
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
            return BSONSerializer.CanBeDeserialized(t, new HashSet<Type>());
        }

        /// <summary>
        /// This is a helper method for the public CanBeDeserialized that avoids infinite 
        /// recursion by tracking which types are already being checked and not checking them.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="checkStack"></param>
        /// <returns></returns>
        public static bool CanBeDeserialized(Type t, HashSet<Type> searchStack)
        {
            bool retval = true;

            #region Lists are special, do special work
            bool isSafeList = true;
            if (t.IsGenericType && t.FullName.StartsWith("System.Collections.Generic.List`1"))
            {
                isSafeList = t.GetGenericArguments().All(y => BSONSerializer.CanBeDeserialized(y));
                retval = isSafeList;
                if (isSafeList)
                {
                    BSONSerializer._canDeserialize.Add(t);
                }
                else
                {
                    BSONSerializer._prohibittedTypes.Add(t);
                }
            }
            #endregion

            #region check properties.
            if (!BSONSerializer._canDeserialize.Contains(t) &&
                !BSONSerializer._prohibittedTypes.Contains(t))
            {
                ///we only care about public properties on instances, not statics.
                foreach (var pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    retval &= pi.CanWrite & pi.CanWrite;
                    var propType = pi.PropertyType;
                    if (!searchStack.Contains(propType))
                    {
                        searchStack.Add(propType);
                        if (!propType.IsValueType)
                        {
                            retval &= BSONSerializer.CanBeDeserialized(propType, searchStack);
                        }
                    }
                }
            }
            else if (BSONSerializer._prohibittedTypes.Contains(t))
            {
                retval = false;
            }
            #endregion

            //if we get all the way to the end, this type is "safe" and we should include actions.
            if (retval == true && !BSONSerializer._canDeserialize.Contains(t))
            {
                BSONSerializer._canDeserialize.Add(t);
                BSONSerializer._allowedTypes.Add(t);
            }
            return retval;
        }

        /// <summary>
        /// Retrieve the property types and names for a given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static IDictionary<String, Type> GetPropertyTypes(Type t)
        {
            Dictionary<String, Type> retval = null;
            if (!BSONSerializer._propertyTypes.TryGetValue(t, out retval))
            {
                retval = new Dictionary<String, Type>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var p in t.GetProperties().OfType<PropertyInfo>()
                    .Where(y => y.GetIndexParameters().Count() == 0))
                {
                    if (p.CanRead && p.CanWrite)
                    {
                        retval[p.Name] = p.PropertyType;
                    }
                }
                BSONSerializer._propertyTypes[t] = retval;
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
        private static IDictionary<String, Action<object, object>> SettersForType(Type documentType)
        {

            if (!BSONSerializer._setters.ContainsKey(documentType))
            {
                BSONSerializer._setters[documentType] = new Dictionary<string, Action<object, object>>(StringComparer.InvariantCultureIgnoreCase);
                if (!typeof(IList).IsAssignableFrom(documentType))
                {
                    foreach (var p in documentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(y => y.GetIndexParameters().Count() == 0 && !y.GetCustomAttributes(true)
                            .Any(f=>f is MongoIgnoreAttribute)))
                    {
                        BSONSerializer._setters[documentType][p.Name] = ReflectionHelpers.SetterMethod(p);
                    }
                }
            }
            return new Dictionary<String, Action<object, object>>(BSONSerializer._setters[documentType],
                StringComparer.InvariantCultureIgnoreCase);
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
                foreach (var p in documentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(y => y.GetIndexParameters().Count() == 0 && 
                        !y.GetCustomAttributes(true).Any(f=>f is MongoIgnoreAttribute)))
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
                (props != null && props.AllProperties()
                .Any(y => y.Value != null && !BSONSerializer.CanBeSerialized(y.Value.GetType()))))
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
            //type + name + data
            List<byte[]> retval = new List<byte[]>(4);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);
            retval.Add(new byte[0]);


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

                var bytes = ((String)value).CStringBytes();
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
            else if (value is OID)
            {
                retval[0] = new byte[] { (byte)BSONTypes.MongoOID };
                var oid = (OID)value;
                retval[3] = oid.Value;
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
                scopedCode[0] = BitConverter.GetBytes(scopedCode.Sum(y => y.Length));
                scopedCode[1] = BitConverter.GetBytes(scopedCode[2].Length);
            }
            else if (value is DateTime?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.DateTime };
                retval[3] = BitConverter.GetBytes((long)(((DateTime?)value).Value - EPOCH).TotalMilliseconds);
            }
            else if (value is long?)
            {
                retval[0] = new byte[] { (byte)BSONTypes.Int64 };
                retval[3] = BitConverter.GetBytes((long)value);
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

                retval[3] = BitConverter.GetBytes(4 + memberBytes.Length + 1)
                    .Concat(memberBytes).Concat(new byte[1]).ToArray();
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

        #endregion

        #region Deserialization
        /// <summary>
        /// converts a binary stream to an object, 
        /// and outs any associated properties to an object that don't map to the class definition of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        public static T Deserialize<T>(BinaryReader stream, ref IDictionary<WeakReference, Flyweight> outProps) 
        {
            return (T)BSONSerializer.Deserialize(stream, ref outProps, typeof(T));
        }

        /// <summary>
        /// Overload that constructs a BinaryReader in memory and then deserializes the values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectData"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData, ref IDictionary<WeakReference, Flyweight> outProps) 
        {
            var ms = new MemoryStream();
            ms.Write(objectData, 0, objectData.Length);
            ms.Position = 0;
            return BSONSerializer.Deserialize<T>(new BinaryReader(ms), ref outProps);
        }

        /// <summary>
        /// Deserialize an object, and don't worry about getting any of the expando props back.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectData"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData) where T : class, new()
        {
            IDictionary<WeakReference, Flyweight> outprops = new Dictionary<WeakReference, Flyweight>();
            return BSONSerializer.Deserialize<T>(objectData, ref outprops);
        }

        /// <summary>
        /// Converts a document's byte-form back into a POCO.
        /// </summary>
        /// <typeparam name="T">The type to be converted back to.</typeparam>
        /// <param name="stream">the document's bytes</param>
        /// <param name="outProps">Properties that don't map onto T.</param>
        /// <returns></returns>
        private static object Deserialize(BinaryReader stream, ref IDictionary<WeakReference, Flyweight> outProps, Type returnType)
        {
            object retval = null;
            if (!BSONSerializer.CanBeDeserialized(returnType))
            {
                throw new NotSupportedException("This type cannot be DESERIALIZED using the BSONSerializer");
            }

            Flyweight extraProps = new Flyweight();

            int length = stream.ReadInt32();
            //get the object length minus the header and the null (5)
            byte[] buffer = new byte[length - 5];
            stream.Read(buffer, 0, length - 5);
            //push the position forward past the null terminator.
            stream.Read(new byte[1], 0, 1);
            retval = Activator.CreateInstance(returnType);

            var setters = BSONSerializer.SettersForType(retval.GetType());

            #region Read out of the buffer.
            while (buffer.Length > 0)
            {
                BSONTypes t = (BSONTypes)buffer[0];
                var keyStringBytes = buffer.Skip(1).TakeWhile(y => y != (byte)0).ToArray();


                Object obj = null;
                String key = Encoding.UTF8.GetString(keyStringBytes);
                var propTypes = BSONSerializer.GetPropertyTypes(returnType);

                Type propType;
                if (!propTypes.TryGetValue(key, out propType))
                {
                    propType = typeof(Flyweight);
                }
                var objectData = buffer.Skip(keyStringBytes.Length + CODE_LENGTH + KEY_TERMINATOR_LENGTH).ToArray();

                int usedBytes;
                obj = BSONSerializer.DeserializeMember(t, objectData, propType, out usedBytes, ref outProps);

                //skip type, the key, the null, the object data
                buffer = buffer.Skip(CODE_LENGTH + keyStringBytes.Length +
                    KEY_TERMINATOR_LENGTH + usedBytes).ToArray();

                if (key == "$err")
                {
                    throw new Exception((obj ?? "").ToString());
                }
                if (retval is Flyweight)
                {
                    ((Flyweight)retval).Set(key, obj);
                }
                else if (setters.ContainsKey(key))
                {
                    var prop = setters[key];
                    prop.Invoke(retval, obj);
                }
                else
                {
                    extraProps[key] = obj;
                }
            }
            #endregion

            if (retval != null)
            {
                outProps[new WeakReference(retval)] = extraProps;
            }
            return retval;
        }

        /// <summary>
        /// Hydrates the given member and returns it, also outs the number of bytes used.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="objectData"></param>
        /// <param name="propertyType"></param>
        /// <param name="usedBytes"></param>
        /// <returns></returns>
        private static object DeserializeMember(BSONTypes t, byte[] objectData, Type proptype, out int usedBytes, ref IDictionary<WeakReference, Flyweight> outProps)
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
                if (binaryType == 2)//general binary.
                {
                    var binaryLength = BitConverter.ToInt32(objectData, 5);
                    retval = objectData.Skip(9).Take(binaryLength).ToArray();
                    usedBytes = binaryLength + 9;
                }
                else if (binaryType == 3)//UUID
                {
                    retval = new Guid(objectData.Skip(5).Take(16).ToArray());
                    usedBytes = 21;
                }
            }
            else if (t == BSONTypes.MongoOID)
            {
                retval = new OID() { Value = objectData.Take(12).ToArray() };
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
                var millisSinceEpoch = BitConverter.ToInt64(objectData.ToArray(), 0);
                retval = BSONSerializer.EPOCH.AddMilliseconds(millisSinceEpoch);
                usedBytes = 8;
            }
            else if (t == BSONTypes.Int64)
            {
                retval = BitConverter.ToInt64(objectData, 0);
                usedBytes = 8;
            }
            else if (t == BSONTypes.Array)
            {
                int length = BitConverter.ToInt32(objectData, 0);
                var list = Activator.CreateInstance(proptype) as IList;
                var type = proptype.GetGenericArguments().First();

                var arrayData = objectData.Take(length - 1).Skip(4).ToArray();

                while (arrayData.Length > 0)
                {
                    //skip the cursor, and then take bytes while not null (index name) and then add the null terminator.
                    BSONTypes memberType = (BSONTypes)arrayData[0];
                    var keyStringBytes = arrayData.Skip(1).TakeWhile(y => y != 0).ToArray();
                    arrayData = arrayData.Skip(2 + keyStringBytes.Length).ToArray();

                    int usedCount = 0;
                    var outobj = BSONSerializer.DeserializeMember(memberType,
                        arrayData.ToArray(), type, out usedCount, ref outProps);
                    list.Add(outobj);

                    arrayData = arrayData.Skip(usedCount).ToArray();
                }

                usedBytes = length;
                retval = list;
            }
            else if (t == BSONTypes.Object)
            {
                int length = BitConverter.ToInt32(objectData, 0);

                var outObj = BSONSerializer.Deserialize(new BinaryReader(new MemoryStream(objectData.Take(length).ToArray())),
                    ref outProps, proptype);
                usedBytes = length;
                retval = outObj;
            }

            return retval;
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
    }
}