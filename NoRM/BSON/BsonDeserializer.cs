namespace NoRM.BSON
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    public class BsonDeserializer
    {
        private static readonly Type _IEnumerableType = typeof(IEnumerable);

        private readonly static IDictionary<BSONTypes, Type> _typeMap = new Dictionary<BSONTypes, Type>
         {
             {BSONTypes.Int32, typeof(int)}, {BSONTypes.Int64, typeof (long)}, {BSONTypes.Boolean, typeof (bool)}, {BSONTypes.String, typeof (string)},
             {BSONTypes.Double, typeof(double)}, {BSONTypes.Binary, typeof (byte[])}, {BSONTypes.Regex, typeof (Regex)}, {BSONTypes.DateTime, typeof (DateTime)},
             {BSONTypes.MongoOID, typeof(ObjectId)}
         };     
        
        public static T Deserialize<T>(byte[] objectData) where T : class, new()
        {
            IDictionary<WeakReference, Flyweight> outprops = new Dictionary<WeakReference, Flyweight>();
            return Deserialize<T>(objectData, ref outprops);
        }
        public static T Deserialize<T>(byte[] objectData, ref IDictionary<WeakReference, Flyweight> outProps)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(objectData, 0, objectData.Length);
                ms.Position = 0;
                return Deserialize<T>(new BinaryReader(ms));
            }
        }
        private static T Deserialize<T>(BinaryReader stream)
        {
            return new BsonDeserializer(stream).Read<T>();
        }

        private readonly BinaryReader _reader;
        private Document _current;

        private BsonDeserializer(BinaryReader reader)
        {            
            _reader = reader;             
        }

        private T Read<T>()
        {
            NewDocument(_reader.ReadInt32());            
            var @object = (T)DeserializeValue(typeof(T), BSONTypes.Object);            
            return @object;
        }

        private void Read(int read)
        {            
            _current.Read += read;
        }
        private bool IsDone()
        {
            var isDone = _current.Read + 1 == _current.Length;
            if (isDone)
            {
                _reader.ReadByte(); // EOO
                var old = _current;
                _current = old.Parent;
                if (_current != null){ Read(old.Length); }
            }
            return isDone;
        }
        private void NewDocument(int length)
        {
            var old = _current;
            _current = new Document {Length = length, Parent = old, Read = 4};            
        }
        private object DeserializeValue(Type type, BSONTypes storedType)
        {
            return DeserializeValue(type, storedType, null);
        }
        private object DeserializeValue(Type type, BSONTypes storedType, IList container)
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
            if (type == typeof(Flyweight))
            {
                return ReadFlyweight();
            }
           
            return ReadObject(type);            
        }

        private object ReadObject(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var typeHelper = TypeHelper.GetHelperForType(type);
            while (true)
            {
                var storageType = ReadType();                
                var name = ReadName();
                if (name == "$err" || name == "errmsg")
                {
                    HandleError((string)DeserializeValue(typeof (string), BSONTypes.String));                    
                }
                var property = (name == "_id") ? typeHelper.FindIdProperty() : typeHelper.FindProperty(name);                
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                IList container = null;
                if (property.Setter == null)
                {
                    var o = property.Getter(instance);
                    container = o is IList ? (IList)o : null; 
                }
                var value = DeserializeValue(property.Type, storageType, container); 
                if (container == null)
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
        private object ReadList(Type listType, IList container)
        {
            NewDocument(_reader.ReadInt32());

            var isReadonly = false;
            var itemType = ListHelper.GetListItemType(listType);
            if (container == null)
            {
                container = ListHelper.CreateContainer(listType, itemType, out isReadonly);
            }
            while (!IsDone())
            {
                var storageType = ReadType();
                
                ReadName();
                if (storageType == BSONTypes.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var value = DeserializeValue(itemType, storageType);
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
            throw new MongoException("No support for binary type: " + subType);
        }
        private string ReadName()
        {
            var buffer = new List<byte>(128); //todo: use a pool to prevent fragmentation
            byte b;
            while ((b = _reader.ReadByte()) > 0)
            {
                buffer.Add(b);                
            }
            Read(buffer.Count+1);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }
        private string ReadString()
        {
            var length = _reader.ReadInt32();
            var buffer = _reader.ReadBytes(length - 1); //todo: again, look at fragementation prevention
            _reader.ReadByte(); //null;
            Read(4 + length);

            return Encoding.UTF8.GetString(buffer);
        }
        private int ReadInt(BSONTypes storedType)
        {
            switch(storedType)
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
        private object ReadEnum(Type type, BSONTypes storedType)
        {
            if (storedType == BSONTypes.Int64)
            {
                return Enum.Parse(type, ReadLong(storedType).ToString(), false);
            }
            return Enum.Parse(type, ReadInt(storedType).ToString(), false);            
        }
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
        private BSONTypes ReadType()
        {
            Read(1);
            return (BSONTypes)_reader.ReadByte();
        }
        private ScopedCode ReadScopedCode()
        {
            _reader.ReadInt32(); //length
            Read(4);
            var name = ReadString();
            NewDocument(_reader.ReadInt32());                       
            return new ScopedCode { CodeString = name, Scope = DeserializeValue(typeof(Flyweight), BSONTypes.Object) };            
        }
        private Flyweight ReadFlyweight()
        {
            var flyweight = new Flyweight();
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

        private static void HandleError(string message)
        {
            throw new MongoException(message);
        }


        private class Document
        {
            public int Length;
            public int Read;
            public Document Parent;
        }

    }
}