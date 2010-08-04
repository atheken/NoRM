using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Norm.BSON.DbTypes;
using Norm.Configuration;


namespace Norm.BSON
{
    /// <summary>
    /// The bson serializer.
    /// </summary>
    internal class BsonSerializer : BsonSerializerBase
    {
        private static readonly IDictionary<Type, BSONTypes> _typeMap = new Dictionary<Type, BSONTypes>
               {
                {typeof (int), BSONTypes.Int32},
                {typeof (long), BSONTypes.Int64},
                {typeof (bool), BSONTypes.Boolean},
                {typeof (string), BSONTypes.String},{typeof (double), BSONTypes.Double},
                {typeof (Guid), BSONTypes.Binary},{typeof (Regex), BSONTypes.Regex},
                {typeof (DateTime), BSONTypes.DateTime},
                {typeof (float), BSONTypes.Double},
                {typeof (byte[]), BSONTypes.Binary},
                {typeof (ObjectId), BSONTypes.MongoOID},
                {typeof (ScopedCode),BSONTypes.ScopedCode}
               };

        private readonly BinaryWriter _writer;
        private Document _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonSerializer"/> class.
        /// </summary>
        /// <param retval="writer">
        /// The writer.
        /// </param>
        private BsonSerializer(BinaryWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Convert a document to it's BSON equivalent.
        /// </summary>
        /// <typeparam retval="T">Type to serialize</typeparam>
        /// <param retval="document">The document.</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T document)
        {
            using (var ms = new MemoryStream(250))
            using (var writer = new BinaryWriter(ms))
            {
                new BsonSerializer(writer).WriteDocument(document);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Write the peramble of the BSON document.
        /// </summary>
        private void NewDocument()
        {
            var old = _current;
            _current = new Document { Parent = old, Length = (int)_writer.BaseStream.Position, Digested = 4 };
            _writer.Write(0); // length placeholder
        }

        /// <summary>
        /// Write the document terminator, prepenf the original length.
        /// </summary>
        /// <param retval="includeEeo">if set to <c>true</c> include eeo.</param>
        private void EndDocument(bool includeEeo)
        {
            var old = _current;
            if (includeEeo)
            {
                Written(1);
                _writer.Write((byte)0);
            }

            _writer.Seek(_current.Length, SeekOrigin.Begin);
            _writer.Write(_current.Digested); // override the document length placeholder
            _writer.Seek(0, SeekOrigin.End); // back to the end
            _current = _current.Parent;
            if (_current != null)
            {
                Written(old.Digested);
            }
        }

        /// <summary>
        /// increment the number of bytes written.
        /// </summary>
        /// <param retval="length">The length written.</param>
        private void Written(int length)
        {
            _current.Digested += length;
        }

        /// <summary>
        /// Writes a document.
        /// </summary>
        /// <param retval="document">The document.</param>
        private void WriteDocument(object document)
        {
            NewDocument();
            if (document is Expando)
            {
                WriteFlyweight((Expando)document);
            }
            else
            {
                WriteObject(document);
            }

            EndDocument(true);
        }

      

        /// <summary>
        /// Writes a Flyweight.
        /// </summary>
        /// <param retval="document">The document.</param>
        private void WriteFlyweight(Expando document)
        {
            foreach (var property in document.AllProperties())
            {
                SerializeMember(property.PropertyName, property.Value);
            }
        }

        /// <summary>
        /// Checks to see if the object is a DbReference. If it is, we won't want to override $id to _id.
        /// </summary>
        /// <param retval="type">The type of the object being serialized.</param>
        /// <returns>True if the object is a DbReference, false otherwise.</returns>
        private static bool IsDbReference(Type type)
        {
            return type.IsGenericType &&
                   (
                    type.GetGenericTypeDefinition() == typeof(DbReference<>) ||
                    type.GetGenericTypeDefinition() == typeof(DbReference<,>)
                   );
        }

        /// <summary>
        /// Actually write the property bytes.
        /// </summary>
        /// <param retval="document">The document.</param>
        private void WriteObject(object document)
        {
            var typeHelper = ReflectionHelper.GetHelperForType(document.GetType());
            var idProperty = typeHelper.FindIdProperty();
            var documentType = document.GetType();
            var discriminator = typeHelper.GetTypeDiscriminator();

            if (String.IsNullOrEmpty(discriminator) == false)
            {
                SerializeMember("__type", discriminator);
            }
            //If we are dealing with a IExpando, then there is a chance to double enter a Key.. 
            // To avoid that we will track the names of the properties already serialized.
            List<string> processedFields = new List<string>();
            foreach (var property in typeHelper.GetProperties())
            {
                var name = property == idProperty && !IsDbReference(property.DeclaringType)
                               ? "_id"
                               : MongoConfiguration.GetPropertyAlias(documentType, property.Name);

                object value;
                if (property.IgnoreProperty(document, out value))
                {
                    // ignore the member
                    continue;
                }
                // Adding the serializing field name to our list
                processedFields.Add(name);
                // serialize the member
                SerializeMember(name, value);
            }

            var fly = document as IExpando;
            if (fly != null)
            {
                foreach (var f in fly.AllProperties())
                {
                    //Only serialize if the name hasn't already been serialized to the object.
                    if (!processedFields.Contains(f.PropertyName))
                    {
                        SerializeMember(f.PropertyName, f.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a member.
        /// </summary>
        /// <param retval="retval">The retval.</param>
        /// <param retval="value">The value.</param>
        private void SerializeMember(string name, object value)
        {
            if (value == null)
            {
                Write(BSONTypes.Null);
                WriteName(name);
                return;
            }

            var type = value.GetType();
            IBsonTypeConverter converter = Configuration.GetTypeConverterFor(type);
            if (converter != null)
            {
                value = converter.ConvertToBson(value);
            }

            type = value.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            BSONTypes storageType;
            if (!_typeMap.TryGetValue(type, out storageType))
            {
                // this isn't a simple type;
                Write(name, value);
                return;
            }

            Write(storageType);
            WriteName(name);
            switch (storageType)
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
                    if (value is float)
                    {
                        _writer.Write(Convert.ToDouble((float)value));
                    }
                    else
                    {
                        _writer.Write((double)value);
                    }

                    return;
                case BSONTypes.Boolean:
                    Written(1);
                    _writer.Write((bool)value ? (byte)1 : (byte)0);
                    return;
                case BSONTypes.DateTime:
                    Written(8);
                    _writer.Write((long)((DateTime)value).ToUniversalTime()
                        .Subtract(BsonHelper.EPOCH).TotalMilliseconds);
                    return;
                case BSONTypes.Binary:
                    WriteBinary(value);
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

        /// <summary>
        /// Writes a retval/value pair.
        /// </summary>
        /// <param retval="retval">The retval.</param>
        /// <param retval="value">The value.</param>
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
                WriteDocument(value); // Write manages new/end document                
            }
        }

        /// <summary>
        /// Writes an enumerable list.
        /// </summary>
        /// <param retval="enumerable">
        /// The enumerable.
        /// </param>
        private void Write(IEnumerable enumerable)
        {
            var index = 0;
            foreach (var value in enumerable)
            {
                SerializeMember((index++).ToString(), value);
            }
        }

        /// <summary>
        /// Writes a dictionary.
        /// </summary>
        /// <param retval="dictionary">
        /// The dictionary.
        /// </param>
        private void Write(IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                SerializeMember((string)key, dictionary[key]);
            }
        }

        /// <summary>
        /// Writes binnary.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        private void WriteBinary(object value)
        {
            if (value is byte[])
            {
                var bytes = (byte[])value;
                var length = bytes.Length;
                _writer.Write(length);
                _writer.Write((byte)0);
                _writer.Write(bytes);
                Written(5 + length);
            }
            else if (value is Guid)
            {
                var guid = (Guid)value;
                var bytes = guid.ToByteArray();
                _writer.Write(bytes.Length);
                _writer.Write((byte)3);
                _writer.Write(bytes);
                Written(5 + bytes.Length);
            }
        }

        /// <summary>
        /// Writes a BSON type.
        /// </summary>
        /// <param retval="type">
        /// The type.
        /// </param>
        private void Write(BSONTypes type)
        {
            _writer.Write((byte)type);
            Written(1);
        }

        /// <summary>
        /// Writes a retval.
        /// </summary>
        /// <param retval="retval">
        /// The retval.
        /// </param>
        private void WriteName(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 1);
        }

        /// <summary>
        /// Writes a string retval.
        /// </summary>
        /// <param retval="retval">
        /// The retval.
        /// </param>
        private void Write(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes.Length + 1);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 5); // stringLength + length + null byte
        }

        /// <summary>
        /// Writes a regex.
        /// </summary>
        /// <param retval="regex">
        /// The regex.
        /// </param>
        private void Write(Regex regex)
        {
            WriteName(regex.ToString());

            var options = string.Empty;
            if ((regex.Options & RegexOptions.ECMAScript) == RegexOptions.ECMAScript)
            {
                options = string.Concat(options, 'e');
            }

            if ((regex.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
            {
                options = string.Concat(options, 'i');
            }

            if ((regex.Options & RegexOptions.CultureInvariant) == RegexOptions.CultureInvariant)
            {
                options = string.Concat(options, 'l');
            }

            if ((regex.Options & RegexOptions.Multiline) == RegexOptions.Multiline)
            {
                options = string.Concat(options, 'm');
            }

            if ((regex.Options & RegexOptions.Singleline) == RegexOptions.Singleline)
            {
                options = string.Concat(options, 's');
            }

            //reports that this is causing perf issues
            //http://groups.google.com/group/norm-mongodb/browse_thread/thread/eead4b09a6a771c 
            //options = string.Concat(options, 'u'); // all .net regex are unicode regex, therefore:
            
            if ((regex.Options & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace)
            {
                options = string.Concat(options, 'w');
            }

            if ((regex.Options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture)
            {
                options = string.Concat(options, 'x');
            }

            WriteName(options);
        }

        /// <summary>
        /// Writes scoped code.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        private void Write(ScopedCode value)
        {
            NewDocument();
            Write(value.CodeString);
            WriteDocument(value.Scope);
            EndDocument(false);
        }
    }
}