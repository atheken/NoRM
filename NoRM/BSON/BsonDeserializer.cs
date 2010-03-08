using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NoRM.BSON
{
    /// <summary>
    /// Converts the BSON Representation of an object back into a POCO.
    /// </summary>
    public class BsonDeserializer
    {
        private static readonly Type _IEnumerableType = typeof(IEnumerable);

        private readonly static IDictionary<BSONTypes, Type> _typeMap = new Dictionary<BSONTypes, Type>{
            #region values
		     {BSONTypes.Int32, typeof(int)},
             {BSONTypes.Int64, typeof (long)}, 
             {BSONTypes.Boolean, typeof (bool)},
             {BSONTypes.String, typeof (string)},
             {BSONTypes.Double, typeof(double)},
             {BSONTypes.Binary, typeof (byte[])}, 
             {BSONTypes.Regex, typeof (Regex)}, 
             {BSONTypes.DateTime, typeof (DateTime)},
             {BSONTypes.MongoOID, typeof(ObjectId)} 
	        #endregion
         };

        /// <summary>
        /// Read a document out of the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectData"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData) where T : class, new()
        {
            IDictionary<WeakReference, Flyweight> outprops = new Dictionary<WeakReference, Flyweight>();
            return Deserialize<T>(objectData, ref outprops);
        }

        /// <summary>
        /// Read a document out of the stram.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectData"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(objectData, 0, objectData.Length);
                ms.Position = 0;
                return Deserialize<T>(new BinaryReader(ms), ref  outProps);
            }
        }

        /// <summary>
        /// Read a document out of the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private static T Deserialize<T>(BinaryReader stream, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            return new BsonDeserializer(stream).Read<T>(ref outProps);
        }

        private readonly BinaryReader _reader;
        private Document _current;

        private BsonDeserializer(BinaryReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Read a document out of the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private T Read<T>(ref IDictionary<WeakReference, Flyweight> outProps)
        {
            NewDocument(_reader.ReadInt32());
            var obj = (T)DeserializeValue(typeof(T), BSONTypes.Object, ref outProps);
            return obj;
        }

        /// <summary>
        /// increment the read counter.
        /// </summary>
        /// <param name="read"></param>
        private void Read(int read)
        {
            _current.Read += read;
        }

        /// <summary>
        /// Are we done yet?
        /// </summary>
        /// <returns></returns>
        private bool IsDone()
        {
            var isDone = _current.Read + 1 == _current.Length;
            if (isDone)
            {
                _reader.ReadByte(); // EOO
                var old = _current;
                _current = old.Parent;
                if (_current != null) { Read(old.Length); }
            }
            return isDone;
        }

        /// <summary>
        /// Begin deserialzation by reading the peramble.
        /// </summary>
        /// <param name="length"></param>
        private void NewDocument(int length)
        {
            var old = _current;
            _current = new Document { Length = length, Parent = old, Read = 4 };
        }

        /// <summary>
        /// Hydrate a member from the stream.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storedType"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private object DeserializeValue(Type type, BSONTypes storedType, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            if (storedType == BSONTypes.Null)
            {
                return null;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }
            if (type == typeof(string))
            {
                return ReadString();
            }
            if (type == typeof(int))
            {
                return ReadInt(storedType);
            }
            if (type.IsEnum)
            {
                return ReadEnum(type, storedType);
            }
            if (type == typeof(float))
            {
                Read(8);
                return (float)_reader.ReadDouble();
            }
            if (storedType == BSONTypes.Binary)
            {
                return ReadBinary();
            }
            if (_IEnumerableType.IsAssignableFrom(type))
            {
                return this.ReadList(type, ref outProps);
            }
            if (type == typeof(bool))
            {
                Read(1);
                return _reader.ReadBoolean();
            }
            if (type == typeof(DateTime))
            {
                return BsonHelper.EPOCH.AddMilliseconds(ReadLong(BSONTypes.Int64));
            }
            if (type == typeof(ObjectId))
            {
                Read(12);
                return new ObjectId(_reader.ReadBytes(12));
            }


            if (type == typeof(long))
            {
                return ReadLong(storedType);
            }
            if (type == typeof(double))
            {
                Read(8);
                return _reader.ReadDouble();
            }
            if (type == typeof(Regex))
            {
                return ReadRegularExpression();
            }
            if (type == typeof(ScopedCode))
            {
                return ReadScopedCode(ref outProps);
            }
            if (type == typeof(Flyweight))
            {
                return ReadFlyweight(ref outProps);
            }

            return ReadObject(type, ref outProps);
        }

        /// <summary>
        /// Hydrate an object from the stream.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private object ReadObject(Type type, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            var instance = Activator.CreateInstance(type);
            var typeHelper = TypeHelper.GetHelperForType(type);
            var addedProps = new Flyweight();
            var addedPropsAny = false;
            while (true)
            {
                var storageType = ReadType();
                var name = ReadName();
                if (name == "$err" || name == "errmsg")
                {
                    HandleError((string)this.DeserializeValue(typeof(string), BSONTypes.String, ref outProps));
                }
                var property = (name == "_id") ? typeHelper.FindIdProperty() : typeHelper.FindProperty(name);
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var value = DeserializeValue(property.Type, storageType, ref outProps);
                if (property != null)
                {
                    property.Setter(instance, value);
                }
                else
                {
                    addedPropsAny = true;
                    addedProps[name] = value;
                }
                if (IsDone())
                {
                    break;
                }
            }
            if (addedPropsAny)
            {
                outProps[new WeakReference(instance)] = addedProps;
            }
            return instance;
        }

        /// <summary>
        /// Deserializes a list.
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private object ReadList(Type listType, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            this.NewDocument(_reader.ReadInt32());
            var itemType = ListHelper.GetListItemType(listType);
            bool isReadonly;
            var container = ListHelper.CreateContainer(listType, itemType, out isReadonly);
            while (!IsDone())
            {
                var storageType = ReadType();

                ReadName();
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var value = this.DeserializeValue(itemType, storageType, ref outProps);
                container.Add(value);
            }
            if (listType.IsArray)
            {
                return ListHelper.ToArray((List<object>)container, itemType);
            }
            if (isReadonly)
            {
                return listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { container.GetType() }, null).Invoke(new object[] { container });
            }
            return container;
        }

        /// <summary>
        /// Deserializes a binary member (UUID, or just byte[] for now)
        /// </summary>
        /// <returns></returns>
        private object ReadBinary()
        {
            var length = _reader.ReadInt32();
            var subType = _reader.ReadByte();
            Read(5 + length);
            if (subType == 2)
            {
                return _reader.ReadBytes(_reader.ReadInt32());
            }
            if (subType == 3)
            {
                return new Guid(_reader.ReadBytes(length));
            }
            //TODO: add MD5 support.
            throw new MongoException("No support for binary type: " + subType);
        }

        /// <summary>
        /// Get the name of the property.
        /// </summary>
        /// <returns></returns>
        private string ReadName()
        {
            var buffer = new List<byte>(128); //todo: use a pool to prevent fragmentation
            byte b;
            while ((b = _reader.ReadByte()) > 0)
            {
                buffer.Add(b);
            }
            Read(buffer.Count + 1);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        /// <summary>
        /// Hydrate a string from the stream.
        /// </summary>
        /// <returns></returns>
        private string ReadString()
        {
            var length = _reader.ReadInt32();
            var buffer = _reader.ReadBytes(length - 1); //todo: again, look at fragementation prevention
            _reader.ReadByte(); //null;
            Read(4 + length);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Read a numeric value out of the db.
        /// </summary>
        /// <param name="storedType"></param>
        /// <returns></returns>
        private int ReadInt(BSONTypes storedType)
        {
            switch (storedType)
            {
                case BSONTypes.Int32:
                    Read(4);
                    return _reader.ReadInt32();
                case BSONTypes.Int64:
                    Read(8);
                    return (int)_reader.ReadInt64();
                case BSONTypes.Double:
                    Read(8);
                    return (int)_reader.ReadDouble();
                default:
                    throw new MongoException("Could not create an int from " + storedType);
            }
        }

        /// <summary>
        /// Read a long value out (not necessarily the same type that was serialized)
        /// </summary>
        /// <param name="storedType"></param>
        /// <returns></returns>
        private long ReadLong(BSONTypes storedType)
        {
            switch (storedType)
            {
                case BSONTypes.Int32:
                    Read(4);
                    return _reader.ReadInt32();
                case BSONTypes.Int64:
                    Read(8);
                    return _reader.ReadInt64();
                case BSONTypes.Double:
                    Read(8);
                    return (long)_reader.ReadDouble();
                default:
                    throw new MongoException("Could not create an int64 from " + storedType);
            }
        }

        /// <summary>
        /// Read out the numeric value as an enum.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="storedType"></param>
        /// <returns></returns>
        private object ReadEnum(Type type, BSONTypes storedType)
        {
            if (storedType == BSONTypes.Int64)
            {
                return Enum.Parse(type, ReadLong(storedType).ToString(), false);
            }
            return Enum.Parse(type, ReadInt(storedType).ToString(), false);
        }

        /// <summary>
        /// Deserialize the a Regex.
        /// </summary>
        /// <returns></returns>
        private object ReadRegularExpression()
        {
            var pattern = ReadName();
            var optionsString = ReadName();

            var options = RegexOptions.None;
            if (optionsString.Contains("e")) options = options | RegexOptions.ECMAScript;
            if (optionsString.Contains("i")) options = options | RegexOptions.IgnoreCase;
            if (optionsString.Contains("l")) options = options | RegexOptions.CultureInvariant;
            if (optionsString.Contains("m")) options = options | RegexOptions.Multiline;
            if (optionsString.Contains("s")) options = options | RegexOptions.Singleline;
            if (optionsString.Contains("w")) options = options | RegexOptions.IgnorePatternWhitespace;
            if (optionsString.Contains("x")) options = options | RegexOptions.ExplicitCapture;

            return new Regex(pattern, options);
        }

        /// <summary>
        /// Find out what was serialized.
        /// </summary>
        /// <returns></returns>
        private BSONTypes ReadType()
        {
            Read(1);
            return (BSONTypes)_reader.ReadByte();
        }

        /// <summary>
        /// Read some scoded javascript out of the db.
        /// </summary>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private ScopedCode ReadScopedCode(ref IDictionary<WeakReference, Flyweight> outProps)
        {
            _reader.ReadInt32(); //length
            Read(4);
            var name = ReadString();
            NewDocument(_reader.ReadInt32());
            return new ScopedCode { CodeString = name, Scope = this.DeserializeValue(typeof(Flyweight), BSONTypes.Object, ref outProps) };
        }

        /// <summary>
        /// Read some arbitrary object out as a dictionary type "thing"
        /// </summary>
        /// <param name="outProps"></param>
        /// <returns></returns>
        private Flyweight ReadFlyweight(ref IDictionary<WeakReference, Flyweight> outProps)
        {
            var flyweight = new Flyweight();
            while (true)
            {
                var storageType = ReadType();
                var name = ReadName();
                if (name == "$err" || name == "errmsg")
                {
                    HandleError((string)this.DeserializeValue(typeof(string), BSONTypes.String, ref outProps));
                }
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var propertyType = _typeMap.ContainsKey(storageType) ? _typeMap[storageType] : typeof(object);
                var value = this.DeserializeValue(propertyType, storageType, ref outProps);
                flyweight.Set(name, value);
                if (IsDone())
                {
                    break;
                }
            }
            return flyweight;
        }

        /// <summary>
        /// Throw an exception with a message about why this couldn't be deserialized.
        /// </summary>
        /// <param name="message"></param>
        private static void HandleError(string message)
        {
            throw new MongoException(message);
        }

        /// <summary>
        /// A set of info about a serialized documet.
        /// </summary>
        private class Document
        {
            public int Length;
            public int Read;
            public Document Parent;
        }

    }
}