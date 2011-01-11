using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Norm.Configuration;


namespace Norm.BSON
{
    /// <summary>
    /// BSON Deserializer
    /// </summary>
    public class BsonDeserializer : BsonSerializerBase
    {
        private static readonly Type _IEnumerableType = typeof(IEnumerable);
        private static readonly Type _IDictionaryType = typeof(IDictionary<,>);

        private readonly static IDictionary<BSONTypes, Type> _typeMap = new Dictionary<BSONTypes, Type>
         {
             {BSONTypes.Int32, typeof(int)}, {BSONTypes.Int64, typeof (long)}, {BSONTypes.Boolean, typeof (bool)}, {BSONTypes.String, typeof (string)},
             {BSONTypes.Double, typeof(double)}, {BSONTypes.Binary, typeof (byte[])}, {BSONTypes.Regex, typeof (Regex)}, {BSONTypes.DateTime, typeof (DateTime)},
             {BSONTypes.MongoOID, typeof(ObjectId)}
         };
        private readonly BinaryReader _reader;
        private Document _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDeserializer"/> class.
        /// </summary>
        /// <param retval="reader">The reader.</param>
        private BsonDeserializer(BinaryReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Deserializes the specified object data.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="objectData">The object data.</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData) where T : class
        {
            IDictionary<WeakReference, Expando> outprops = new Dictionary<WeakReference, Expando>();
            return Deserialize<T>(objectData, ref outprops);
        }

        /// <summary>
        /// Deserializes the specified object data.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="objectData">The object data.</param>
        /// <param retval="outProps">The out props.</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] objectData, ref IDictionary<WeakReference, Expando> outProps)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(objectData, 0, objectData.Length);
                ms.Position = 0;
                return Deserialize<T>(new BinaryReader(ms));
            }
        }

        /// <summary>
        /// Deserializes the specified object data.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="objectData">The object data.</param>
        /// <param retval="outProps">The out props.</param>
        /// <returns></returns>
        public static T Deserialize<T>(int length, BinaryReader reader, ref IDictionary<WeakReference, Expando> outProps)
        {
            return Deserialize<T>(reader, length);
        }

        /// <summary>
        /// Deserializes the specified stream.
        /// </summary>
        /// <typeparam retval="T"></typeparam>
        /// <param retval="stream">The stream.</param>
        /// <returns></returns>
        private static T Deserialize<T>(BinaryReader stream)
        {
            return Deserialize<T>(stream, stream.ReadInt32());
        }
        private static T Deserialize<T>(BinaryReader stream, int length)
        {

            var deserializer = new BsonDeserializer(stream);
            T retval = default(T);
            try
            {
                retval = deserializer.Read<T>(length);
            }
            catch (Exception ex)
            {
                int toRead = deserializer._current.Length - deserializer._current.Digested;
                deserializer._reader.ReadBytes(toRead);
                throw ex;
            }

            return retval;
        }


        private T Read<T>(int length)
        {
            NewDocument(length);
            var @object = (T)DeserializeValue(typeof(T), BSONTypes.Object);
            // traverse the object T and apply the DefaultValue to the properties that have them

            return @object;
        }

        /// <summary>
        /// Reads the specified document forward by the input value.
        /// </summary>
        /// <param retval="read">Read length.</param>
        private void Read(int read)
        {
            _current.Digested += read;
        }

        /// <summary>
        /// Determines whether there is more to read.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is read; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDone()
        {
            var isDone = _current.Digested + 1 == _current.Length;
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
        /// Creates a new document.
        /// </summary>
        /// <param retval="length">The document length.</param>
        private void NewDocument(int length)
        {
            var old = _current;
            _current = new Document { Length = length, Parent = old, Digested = 4 };
        }

        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="storedType">Type of the stored.</param>
        /// <returns></returns>
        private object DeserializeValue(Type type, BSONTypes storedType)
        {
            return DeserializeValue(type, storedType, null);
        }

        /// <summary>
        /// Applies optional type conversion and deserializes the value.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="storedType">Type of the stored.</param>
        /// <param retval="container">The container.</param>
        /// <returns></returns>
        private object DeserializeValue(Type type, BSONTypes storedType, object container)
        {
            IBsonTypeConverter converter = Configuration.GetTypeConverterFor(type);
            if (converter != null)
            {
                Type serializedType = converter.SerializedType;
                object value = DeserializeValueAfterConversion(serializedType, storedType, container);
                return converter.ConvertFromBson(value);
            }
            else
            {
                return DeserializeValueAfterConversion(type, storedType, container);
            }
        }


        /// <summary>
        /// Deserializes the value after any type conversion has been applied.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="storedType">Type of the stored.</param>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        private object DeserializeValueAfterConversion(Type type, BSONTypes storedType, object container)
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
            if (_IEnumerableType.IsAssignableFrom(type) || storedType == BSONTypes.Array)
            {
                return ReadList(type, container);
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
                return ReadScopedCode();
            }
            if (type == typeof(Expando))
            {
                return ReadFlyweight();
            }

            return ReadObject(type);
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param retval="type">The object type.</param>
        /// <returns></returns>
        private object ReadObject(Type type)
        {
            bool processedNonTypeProperties = false;
            object instance = null;
            ReflectionHelper typeHelper = null;

            if (type == typeof(Object))
            {
                //override the requested type so that some reasonable things happen.
                type = typeof(Expando);
            }
            if (type.IsInterface == false && type.IsAbstract == false)
            {
                instance = Activator.CreateInstance(type, true);
                typeHelper = ReflectionHelper.GetHelperForType(type);
                typeHelper.ApplyDefaultValues(instance);
            }
            while (true)
            {
                var storageType = ReadType();
                var name = ReadName();
                if (name == "$err" || name == "errmsg")
                {
                    HandleError((string)DeserializeValue(typeof(string), BSONTypes.String));
                }

                // This should work, because the serializer always serialises this property first
                if (name == "__type")
                {
                    if (processedNonTypeProperties)
                        throw new MongoException("Found type declaration after processing properties - data loss would occur - the object has been incorrectly serialized");

                    var typeName = ReadString();
                    type = Type.GetType(typeName, true);
                    typeHelper = ReflectionHelper.GetHelperForType(type);
                    instance = Activator.CreateInstance(type, true);
                    typeHelper.ApplyDefaultValues(instance);
                    continue;
                }

                if (instance == null)
                {
                    throw new MongoException("Could not find the type to instantiate in the document, and " + type.Name + " is an interface or abstract type. Add a MongoDiscriminatedAttribute to the type or base type, or try to work with a concrete type next time.");
                }

                processedNonTypeProperties = true;

                var property = (name == "_id" || name == "$id")
                    ? typeHelper.FindIdProperty()
                    : typeHelper.FindProperty(name);

                if (property == null && !typeHelper.IsExpando)
                {
                    throw new MongoException(string.Format("Deserialization failed: type {0} does not have a property named {1}", type.FullName, name));
                }
                var isNull = false;
                if (storageType == BSONTypes.Object)
                {
                    var length = _reader.ReadInt32();
                    if (length == 5)
                    {
                        _reader.ReadByte(); //eoo
                        Read(5);
                        isNull = true;
                    }
                    else
                    {
                        NewDocument(length);
                    }
                }
                object container = null;
                if (property != null && property.Setter == null)
                {
                    container = property.Getter(instance);
                }
                var propertyType = property != null ? property.Type : _typeMap.ContainsKey(storageType) ? _typeMap[storageType] : typeof(object);
                var value = isNull ? null : DeserializeValue(propertyType, storageType, container);
                if (property == null)
                {
                    ((IExpando)instance)[name] = value;
                }
                else if (container == null && value != null)
                {
                    property.Setter(instance, value);
                }
                if (IsDone())
                {
                    break;
                }
            }
            return instance;
        }

        /// <summary>
        /// Reads a list.
        /// </summary>
        /// <param retval="listType">Type of the list.</param>
        /// <param retval="existingContainer">The existing container.</param>
        /// <returns></returns>
        private object ReadList(Type listType, object existingContainer)
        {
            if (IsDictionary(listType))
            {
                return ReadDictionary(listType, existingContainer);
            }
            //If we just got an untyped object here, we don't know what we have, but we know its an array.
            //So deserialize it to a list of Expandos.
            if (listType == typeof(Object))
            {
                listType = typeof(List<Expando>);
            }

            NewDocument(_reader.ReadInt32());
            var itemType = ListHelper.GetListItemType(listType);
            var isObject = typeof(object) == itemType;
            var wrapper = BaseWrapper.Create(listType, itemType, existingContainer);

            while (!IsDone())
            {
                var storageType = ReadType();
                ReadName();
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var specificItemType = isObject ? _typeMap[storageType] : itemType;
                var value = DeserializeValue(specificItemType, storageType);
                wrapper.Add(value);
            }
            return wrapper.Collection;
        }

        /// <summary>
        /// Determines whether the specified type is a dictionary.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is a dictionary; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDictionary(Type type)
        {
            var types = new List<Type>(type.GetInterfaces());
            types.Insert(0, type);
            foreach (var interfaceType in types)
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Reads a dictionary.
        /// </summary>
        /// <param retval="listType">Type of the list.</param>
        /// <param retval="existingContainer">The existing container.</param>
        /// <returns></returns>
        private object ReadDictionary(Type listType, object existingContainer)
        {
            var valueType = ListHelper.GetDictionarValueType(listType);
            var container = existingContainer == null ?
                ListHelper.CreateDictionary(listType, ListHelper.GetDictionarKeyType(listType), valueType)
                : (IDictionary)existingContainer;

            while (!IsDone())
            {
                var storageType = ReadType();

                var key = ReadName();
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var value = DeserializeValue(valueType, storageType);
                container.Add(key, value);
            }
            return container;
        }

        /// <summary>
        /// Reads binary.
        /// </summary>
        /// <returns></returns>
        private object ReadBinary()
        {
            var length = _reader.ReadInt32();
            var subType = _reader.ReadByte();
            Read(5 + length);
            if (subType == 0)
            {
                return _reader.ReadBytes(length);
            }
            if (subType == 2)
            {
                return _reader.ReadBytes(_reader.ReadInt32());
            }
            if (subType == 3)
            {
                return new Guid(_reader.ReadBytes(length));
            }
            throw new MongoException("No support for binary type: " + subType);
        }

        /// <summary>
        /// Reads a retval.
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
        /// Reads a string.
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
        /// Reads ag int.
        /// </summary>
        /// <param retval="storedType">Type of the stored.</param>
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
        /// Reads a long.
        /// </summary>
        /// <param retval="storedType">Type of the stored.</param>
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
        /// Reads an enum.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="storedType">Type of the stored.</param>
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
        /// Reads the regular expression.
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
        /// Reads a type.
        /// </summary>
        /// <returns></returns>
        private BSONTypes ReadType()
        {
            Read(1);
            return (BSONTypes)_reader.ReadByte();
        }

        /// <summary>
        /// Reads scoped code.
        /// </summary>
        /// <returns></returns>
        private ScopedCode ReadScopedCode()
        {
            _reader.ReadInt32(); //length
            Read(4);
            var name = ReadString();
            NewDocument(_reader.ReadInt32());
            return new ScopedCode { CodeString = name, Scope = DeserializeValue(typeof(Expando), BSONTypes.Object) };
        }

        /// <summary>
        /// Reads a flyweight.
        /// </summary>
        /// <returns></returns>
        private Expando ReadFlyweight()
        {
            var flyweight = new Expando();
            while (true)
            {
                var storageType = ReadType();
                var name = ReadName();
                if (name == "$err" || name == "errmsg")
                {
                    HandleError((string)DeserializeValue(typeof(string), BSONTypes.String));
                }
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var propertyType = _typeMap.ContainsKey(storageType) ? _typeMap[storageType] : typeof(object);
                var value = DeserializeValue(propertyType, storageType);
                flyweight.Set(name, value);
                if (IsDone())
                {
                    break;
                }
            }
            return flyweight;
        }

        /// <summary>
        /// Handles ab error.
        /// </summary>
        /// <param retval="message">The message.</param>
        private static void HandleError(string message)
        {
//TODO: this should flush the rest of the bytes off of the incoming stream, right now this is a BUG!
            throw new MongoException(message);
        }
    }
}
