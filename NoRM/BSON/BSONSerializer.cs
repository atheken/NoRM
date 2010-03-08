using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using NoRM.Configuration;

namespace NoRM.BSON
{
    internal class BsonSerializer
    {
        private readonly static IDictionary<Type, BSONTypes> _typeMap = new Dictionary<Type, BSONTypes>
         {
             {typeof (int), BSONTypes.Int32}, {typeof (long), BSONTypes.Int64}, {typeof (bool), BSONTypes.Boolean}, {typeof (string), BSONTypes.String},
             {typeof(double), BSONTypes.Double}, {typeof (Guid), BSONTypes.Binary}, {typeof (Regex), BSONTypes.Regex}, {typeof (DateTime), BSONTypes.DateTime}, 
             {typeof(float), BSONTypes.Double}, {typeof (byte[]), BSONTypes.Binary}, {typeof(ObjectId), BSONTypes.MongoOID}, {typeof(ScopedCode), BSONTypes.ScopedCode}
         };

        private readonly BinaryWriter _writer;
        private Document _current;

        private BsonSerializer(BinaryWriter writer)
        {
            _writer = writer;
        }
        public static byte[] Serialize<T>(T document)
        {
            using (var ms = new MemoryStream(250))
            using (var writer = new BinaryWriter(ms))
            {
                new BsonSerializer(writer).WriteDocument(document);
                return ms.ToArray();
            }
        }

        private void NewDocument()
        {
            var old = _current;
            _current = new Document { Parent = old, Start = (int)_writer.BaseStream.Position, Written  = 4};
            _writer.Write(0); //length placeholder
        }
        private void EndDocument(bool includeEeo)
        {
            var old = _current;
            if (includeEeo)
            {
                Written(1);
                _writer.Write((byte)0);
            }            
            _writer.Seek(_current.Start, SeekOrigin.Begin);
            _writer.Write(_current.Written); //override the document length placeholder
            _writer.Seek(0, SeekOrigin.End); //back to the end
            _current = _current.Parent;
            if (_current != null)
            {
                Written(old.Written);
            }

        }
        private void Written(int length)
        {
            _current.Written += length;
        }

        private void WriteDocument(object document)
        {
            NewDocument();
            if (document is Flyweight)
            {
                WriteFlyweight((Flyweight) document);
            }
            else
            {
                WriteObject(document);
            }
            EndDocument(true);
        }

        private void WriteFlyweight(Flyweight document)
        {
            foreach (var property in document.AllProperties())
            {
                SerializeMember(property.PropertyName, property.Value);
            }
        }
        private void WriteObject(object document)
        {
            var typeHelper = TypeHelper.GetHelperForType(document.GetType());
            var idProperty = typeHelper.FindIdProperty();
            var documentType = document.GetType();

            foreach (var property in typeHelper.GetProperties())
            {
                var name = property == idProperty 
                    ? "_id"
                    :   MongoConfiguration.GetPropertyAlias(documentType, property.Name);

                var value = property.Getter(document);
                if (value == null && property.IgnoreIfNull)
                {
                    continue;
                }
                SerializeMember(name, value);
            }
        }

        private void SerializeMember(string name, object value)
        {
            if (value == null)
            {
                Write(BSONTypes.Null);
                WriteName(name);
                return;
            }
            var type = value.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            BSONTypes storageType;           
            if (!_typeMap.TryGetValue(type, out storageType))
            {
                //this isn't a simple type;
                Write(name, value);
                return;
            }
            
            Write(storageType);
            WriteName(name);
            switch(storageType)
            {
                case BSONTypes.Int32:
                    Written(4);
                    _writer.Write((int)value);
                    return;
                case BSONTypes.Int64:
                    Written(8);
                    _writer.Write((long)value);
                    return;
                case BSONTypes.String:
                    Write((string)value);
                    return;
                case BSONTypes.Double:
                    Written(8);
                    if (value is float) { _writer.Write(Convert.ToDouble((float)value)); }
                    else { _writer.Write((double)value); }
                    return;
                case BSONTypes.Boolean:
                    Written(1);
                    _writer.Write((bool)value ? (byte)1 : (byte)0);
                    return;
                case BSONTypes.DateTime:
                    Written(8);
                    _writer.Write((long)((DateTime)value).Subtract(BsonHelper.EPOCH).TotalMilliseconds);
                    return;
                case BSONTypes.Binary:
                    WriteBinnary(value);
                    return;
                case BSONTypes.ScopedCode:
                    Write((ScopedCode)value);
                    return;
                case BSONTypes.MongoOID:
                    Written(((ObjectId)value).Value.Length);
                    _writer.Write(((ObjectId)value).Value);
                    return;
                case BSONTypes.Regex:
                    Write((Regex)value);
                    break;
            }
        }

        private void Write(string name, object value)
        {
            if (value is IDictionary)
            {
                Write(BSONTypes.Object);
                WriteName(name);
                NewDocument();
                Write((IDictionary)value);
                EndDocument(true);
            }
            else if (value is IEnumerable)
            {
                Write(BSONTypes.Array);
                WriteName(name);    
                NewDocument();
                Write((IEnumerable)value);                
                EndDocument(true);
            }
            else if (value is ModifierCommand)
            {
                var command = (ModifierCommand)value;
                Write(BSONTypes.Object);
                WriteName(command.CommandName);                
                NewDocument();
                SerializeMember(name, command.ValueForCommand);
                EndDocument(true);                               
            }
            else if (value is QualifierCommand)
            {
                var command = (QualifierCommand)value;
                Write(BSONTypes.Object);
                WriteName(name);
                NewDocument();
                SerializeMember(command.CommandName, command.ValueForCommand);
                EndDocument(true);
            }          
            else 
            {
                Write(BSONTypes.Object);
                WriteName(name);                
                WriteDocument(value); //Write manages new/end document                
            }  
        }
        private void Write(IEnumerable enumerable)
        {
            var index = 0;
            foreach (var value in enumerable)
            {
                SerializeMember(index++.ToString(), value);
            }
        }
        private void Write(IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                SerializeMember((string)key, dictionary[key]);
            }
        }        
        private void WriteBinnary(object value)
        {            
            if (value is byte[])
            {                
                var bytes = (byte[])value;
                var length = bytes.Length;
                _writer.Write(length + 4);
                _writer.Write((byte)2);
                _writer.Write(length);
                _writer.Write(bytes);
                Written(9 + length);
            }
            else if (value is Guid)
            {
                var guid = (Guid) value;
                var bytes = guid.ToByteArray();
                _writer.Write(bytes.Length);
                _writer.Write((byte)3);
                _writer.Write(bytes);
                Written(5 + bytes.Length);
            }
        }
        private void Write(BSONTypes type)
        {
            _writer.Write((byte)type);
            Written(1);
        }
        private void WriteName(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 1);
        }
        private void Write(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes.Length + 1);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 5); //stringLength + length + null byte
        }
        private void Write(Regex regex)
        {
            WriteName(regex.ToString());

            var options = string.Empty;
            if ((regex.Options & RegexOptions.ECMAScript) == RegexOptions.ECMAScript) { options = string.Concat(options, 'e'); }
            if ((regex.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase) { options = string.Concat(options, 'i'); }
            if ((regex.Options & RegexOptions.CultureInvariant) == RegexOptions.CultureInvariant) { options = string.Concat(options, 'l'); }
            if ((regex.Options & RegexOptions.Multiline) == RegexOptions.Multiline) { options = string.Concat(options, 'm'); }
            if ((regex.Options & RegexOptions.Singleline) == RegexOptions.Singleline) { options = string.Concat(options, 's'); }
            options = string.Concat(options, 'u'); //all .net regex are unicode regex, therefore:
            if ((regex.Options & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace) { options = string.Concat(options, 'w'); }
            if ((regex.Options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture) { options = string.Concat(options, 'x'); }
            WriteName(options);            
        }
        private void Write(ScopedCode value)
        {
            NewDocument();
            Write(value.CodeString);
            WriteDocument(value.Scope);
            EndDocument(false);
        }
        
        private class Document
        {
            public int Start;
            public int Written;
            public Document Parent;
        }
    }
}