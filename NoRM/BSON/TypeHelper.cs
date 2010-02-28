namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class TypeHelper
    {
        private static readonly Type _myType = typeof (TypeHelper);
        private readonly Type _type;
        private readonly IDictionary<string, PropertyHelper> _setters;
        public IList<PropertyInfo> Properties { get; private set; }        
        
        public TypeHelper(Type type)
        {
            _type = type;
            Properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);            
            _setters = LoadSetters(FindIdProperty());            

        }

        public PropertyHelper FindSetter(string name)
        {
            if (!_setters.ContainsKey(name))
            {
                throw new MongoException(string.Format("The type {0} doesn't have a property named {1}", _type.FullName, name));
            }
            return _setters[name];
        }
        public PropertyHelper FindIdSetter()
        {
            return _setters["$_id"];
        }
    
        private PropertyInfo FindIdProperty()
        {
            PropertyInfo foundSoFar = null;
            foreach (var property in Properties)
            {
                if (property.GetCustomAttributes(BsonHelper.MongoIdentifierAttribute, true).Length > 0)
                {
                    return property;                    
                }
                if (string.Compare(property.Name, "_id", true) == 0)
                {
                    foundSoFar = property;                    
                }
                if (foundSoFar == null && string.Compare(property.Name, "Id", true) == 0)
                {
                    foundSoFar = property;
                }
            }
            return foundSoFar;
        }
        private IDictionary<string, PropertyHelper> LoadSetters(PropertyInfo idProperty)
        {
            var setters = new Dictionary<string, PropertyHelper>(StringComparer.CurrentCultureIgnoreCase);
            foreach(var property in Properties)
            {
                var name = (property == idProperty) ? "$_id" : property.Name;
                setters.Add(name, new PropertyHelper { Property = property, Setter = CreateSetterMethod(property) });
            }
            return setters;
        }
        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }
        //called via reflection
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method.GetSetMethod());
            return (target, param) => func((TTarget)target, (TParam)param);
        }
    }

    internal class PropertyHelper
    {
        public PropertyInfo Property { get; set; }
        public Action<object, object> Setter { get; set; }
    }
}