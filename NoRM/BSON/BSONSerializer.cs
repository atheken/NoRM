namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections;

    internal class BsonSerializer
    {
        private readonly IDictionary<Type, BSONTypes> _typeMap = new Dictionary<Type, BSONTypes>
         {
             {typeof (int), BSONTypes.Int32}, {typeof (long), BSONTypes.Int64}, {typeof (bool), BSONTypes.Boolean}, {typeof (string), BSONTypes.String},
             {typeof(double), BSONTypes.Double}, {typeof (Guid), BSONTypes.Binary}, {typeof (Regex), BSONTypes.Regex}, {typeof (DateTime), BSONTypes.DateTime}, 
             {typeof (byte[]), BSONTypes.Binary}, {typeof(ObjectId), BSONTypes.MongoOID}
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
                new BsonSerializer(writer).Write(document);
                return ms.ToArray();
            }
        }

        private void NewDocument()
        {
            var old = _current;
            _current = new Document { Parent = old, Start = (int)_writer.BaseStream.Position, Written  = 4};
            _writer.Write(0); //length placeholder
        }
        private void EndDocument()
        {
            var old = _current;
            _writer.Write((byte)0); //EOO
            _writer.Seek(_current.Start, SeekOrigin.Begin);
            _writer.Write(_current.Written+1); //override the document length placeholder (+eeo);
            _writer.Seek(0, SeekOrigin.End); //back to the end
            _current = _current.Parent;
            if (_current != null)
            {
                Written(old.Written+1); //+eeo
            }

        }
        private void Written(int length)
        {
            _current.Written += length;
        }

        private void Write(object document)
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
            EndDocument();
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
            foreach (var property in typeHelper.GetProperties())
            {
                var name = property == idProperty ? "_id" : property.Name;
                var value = property.Getter(document);
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
                    _writer.Write((double)value);
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
            if (value is IEnumerable)
            {
                Write(BSONTypes.Array);
                WriteName(name);    
                NewDocument();
                WriteArray((IEnumerable)value);                
                EndDocument();
            }
            else if (value is ModifierCommand)
            {
                var command = (ModifierCommand)value;
                Write(BSONTypes.Object);
                WriteName(command.CommandName);                
                NewDocument();
                SerializeMember(name, command.ValueForCommand);
                EndDocument();                               
            }
            else if (value is QualifierCommand)
            {
                var command = (QualifierCommand)value;
                Write(BSONTypes.Object);
                WriteName(name);
                NewDocument();
                SerializeMember(command.CommandName, command.ValueForCommand);
                EndDocument();
            }          
            else 
            {
                Write(BSONTypes.Object);
                WriteName(name);                
                Write(value); //Write manages new/end document                
            }
            
                    
        }
        private void WriteArray(IEnumerable enumerable)
        {

            var index = 0;
            foreach(var value in enumerable)
            {
                SerializeMember(index++.ToString(), value);
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



        private class Document
        {
            public int Start;
            public int Written;
            public Document Parent;
        }

        //"special" case.
        /*if (value is ModifierCommand)
        {
            var o = value as ModifierCommand;
            //set type of member.
            retval.Add(new[] {(byte) BSONTypes.Object});
            //set name of member
            retval.Add(CStringBytes(o.CommandName));

            //construct member bytes
            var modValue = new List<byte[]> {new byte[4], SerializeMember(name, o.ValueForCommand), new byte[1]};
            modValue[0] = BitConverter.GetBytes(modValue.Sum(y => y.Length));

            //add this to the main retval.
            retval.AddRange(modValue);
        }
        else if (value is QualifierCommand)
        {
            //wow, this is insane, the idiom for "query" commands is exactly opposite of that for "update" commands.
            var o = value as QualifierCommand;
            //set type of member.
            retval.Add(new[] {(byte) BSONTypes.Object});
            //set name of member
            retval.Add(CStringBytes(name));

            //construct member bytes
            var modValue = new List<byte[]> {new byte[4], SerializeMember(o.CommandName, o.ValueForCommand), new byte[1]};
            modValue[0] = BitConverter.GetBytes(modValue.Sum(y => y.Length)); //add this to the main retval.

            retval.AddRange(modValue);
        }*/
        /*
        private void SerializeMember(string key, object value)
        {
            var isEnum = (value is Enum);
            Type enumType = null;
            if (isEnum)
            {
                enumType = Enum.GetUnderlyingType(value.GetType());
            }

            //type + name + data
            var retval = new List<byte[]>(4) {new byte[0], new byte[0], new byte[0], new byte[0]};


            retval[0] = new[] {(byte) BSONTypes.Null};
            retval[1] = string.Compare(key, "id", true) == 0 ? CStringBytes("_id") : CStringBytes(key);
            retval[2] = new byte[0];
            //this is where the magic occurs
            //HEED MY WARNING! ALWAYS test for NULL first, as other things below ASSUME that if it gets past here, it's NOT NULL!
            if (value == null)
            {
                retval[0] = new[] {(byte) BSONTypes.Null};
                retval[2] = new byte[0];
            }
            else if (value is int || (isEnum && typeof (int).IsAssignableFrom(enumType)))
            {
                retval[0] = new[] {(byte) BSONTypes.Int32};
                retval[2] = BitConverter.GetBytes((int) value);
            }
            else if (value is double || (isEnum && typeof (double).IsAssignableFrom(enumType)))
            {
                retval[0] = new[] {(byte) BSONTypes.Double};
                retval[2] = BitConverter.GetBytes((double) value);
            }
            else if (value is string)
            {
                retval[0] = new[] {(byte) BSONTypes.String};
                //get bytes and append a null to the end.

                var bytes = CStringBytes((string) value);
                retval[2] = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
            }
            else if (value is Regex)
            {
                retval[0] = new[] {(byte) BSONTypes.Regex};
                var rex = (Regex) value;
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

                retval[2] = Encoding.UTF8.GetBytes(pattern).Concat(new byte[1])
                    .Concat(Encoding.UTF8.GetBytes(options)).Concat(new byte[1]).ToArray();
            }
            else if (value is bool)
            {
                retval[0] = new[] {(byte) BSONTypes.Boolean};
                retval[2] = BitConverter.GetBytes((bool) value);
            }
            else if (value is byte[])
            {
                retval[0] = new[] {(byte) BSONTypes.Binary};
                var binary = new List<byte[]> {new byte[0], new[] {(byte) 2}};
                var theBytes = (byte[]) value;
                binary.Add(BitConverter.GetBytes(theBytes.Length)); //describe the number of bytes.
                binary.Add(theBytes); //add the bytes
                binary[0] = BitConverter.GetBytes(binary.Sum(y => y.Length) - 1); //set the total binary size (after the subtype.. weird)
                //not sure if this is correct.
                retval[2] = binary.SelectMany(h => h).ToArray();
            }
            else if (value is Guid)
            {
                retval[0] = new[] {(byte) BSONTypes.Binary};
                var binary = new List<byte[]> {BitConverter.GetBytes(16)};
                var val = (Guid?) value;
                binary.Add(new[] {(byte) 3});
                binary.Add(val.Value.ToByteArray());
                retval[2] = binary.SelectMany(y => y).ToArray();
            }
            else if (value is ObjectId)
            {
                retval[0] = new[] {(byte) BSONTypes.MongoOID};
                var oid = (ObjectId) value;
                retval[2] = oid.Value;
            }
            else if (value is DBReference)
            {
                retval[0] = new[] {(byte) BSONTypes.Reference};
                //TODO: serialize document reference.
            }
            else if (value is ScopedCode)
            {
                retval[0] = new[] {(byte) BSONTypes.ScopedCode};
                var code = value as ScopedCode;
                var scopedCode = new List<byte[]> {new byte[4], new byte[4], CStringBytes(code.CodeString ?? ""), Serialize(code.Scope), new byte[1]};
                scopedCode[0] = BitConverter.GetBytes(scopedCode.Sum(y => y.Length));
                scopedCode[1] = BitConverter.GetBytes(scopedCode[2].Length);
                retval[2] = scopedCode.SelectMany(y => y).ToArray();
            }
            else if (value is DateTime?)
            {
                retval[0] = new[] {(byte) BSONTypes.DateTime};
                retval[2] = BitConverter.GetBytes((long) (((DateTime?) value).Value - BsonHelper.EPOCH).TotalMilliseconds);
            }
            else if (value is long || (isEnum && typeof (long).IsAssignableFrom(enumType)))
            {
                retval[0] = new[] {(byte) BSONTypes.Int64};
                retval[2] = BitConverter.GetBytes((long) value);
            }
                //handle arrays and the like.
            else if (value is IEnumerable)
            {
                retval[0] = new[] {(byte) BSONTypes.Array};
                var index = -1;
                var o = value as IEnumerable;
                var memberBytes = o.OfType<Object>().Select(y =>
                                                                {
                                                                    index++;
                                                                    return SerializeMember(index.ToString(), y);
                                                                }).SelectMany(h => h).ToArray();

                retval[2] = BitConverter.GetBytes(4 + memberBytes.Length + 1).Concat(memberBytes).Concat(new byte[1]).ToArray();
            }
                //TODO: implement something for "Symbol"
                //TODO: implement non-scoped code handling.
            else
            {
                retval[0] = new[] {(byte) BSONTypes.Object};
                retval[2] = Serialize(value);
            }

            return retval.SelectMany(h => h).ToArray();
        }

      
        public static byte[] CStringBytes(string stringToEncode)
        {
            return Encoding.UTF8.GetBytes(stringToEncode).Concat(new byte[1]).ToArray();
        }
        public static void CStringBytes(string stringToEncode, BinaryWriter binaryWriter)
        {
            var bytes = Encoding.UTF8.GetBytes(stringToEncode);
            binaryWriter.Write(bytes.Length + 1);
            binaryWriter.Write(bytes);
            binaryWriter.Write(new byte[1]);
        }*/
    }
}