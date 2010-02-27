namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class TypeHelper
    {
        private readonly Type _type;
        private readonly int _length;
        public IList<PropertyInfo> Properties { get; private set; }
        public PropertyInfo IdProperty { get; private set; }

        public TypeHelper(Type type)
        {
            _type = type;
            Properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            _length = Properties.Count;
            foreach(var property in Properties)
            {
                if (property.GetCustomAttributes(BsonHelper.MongoIdentifierAttribute, true).Length > 0)
                {
                    IdProperty = property;
                    break;
                }
                if (string.Compare(property.Name, "_id", true) == 0)
                {
                    IdProperty = property;
                    continue;
                }
                if (IdProperty == null && string.Compare(property.Name, "Id", true) == 0)
                {
                    IdProperty = property;                   
                }
            }
        }

        public PropertyInfo FindProperty(string name)
        {
            for(var i = 0; i < _length; ++i)
            {
                if (string.Compare(Properties[i].Name, name, true) == 0)
                {
                    return Properties[i];
                }
            }
            throw new MongoException(string.Format("The type {0} doesn't have a property named {1}", _type.FullName, name));
        }
    }
}