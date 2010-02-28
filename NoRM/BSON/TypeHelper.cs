namespace NoRM.BSON
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Attributes;

    internal class TypeHelper
    {
        private static readonly Type _ignoredType = typeof (MongoIgnoreAttribute);
        private readonly Type _type;
        private readonly IDictionary<string, MagicProperty> _properties;
        private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();
        
        public TypeHelper(Type type)
        {
            _type = type;
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            _properties = LoadMagicProperties(properties, IdProperty(properties));            

        }        

        public static TypeHelper GetHelperForType(Type type)
        {
            TypeHelper helper;
            if (!_cachedTypeLookup.TryGetValue(type, out helper))
            {
                helper = new TypeHelper(type);
                _cachedTypeLookup[type] = helper;
            }
            return helper;
        }

        public ICollection<MagicProperty> GetProperties()
        {
            return _properties.Values;
        }
        public MagicProperty FindProperty(string name)
        {
            if (!_properties.ContainsKey(name))
            {
                throw new MongoException(string.Format("The type {0} doesn't have a property named {1}", _type.FullName, name));
            }
            return _properties[name];
        }
        public MagicProperty FindIdProperty()
        {
            return _properties["$_id"];
        }    
        private static PropertyInfo IdProperty(IEnumerable<PropertyInfo> properties)
        {
            PropertyInfo foundSoFar = null;
            foreach (var property in properties)
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
        private static IDictionary<string, MagicProperty> LoadMagicProperties(IEnumerable<PropertyInfo> properties, PropertyInfo idProperty)
        {
            var magic = new Dictionary<string, MagicProperty>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(_ignoredType, true).Length > 0)
                {
                    continue;
                }
                var name = (property == idProperty) ? "$_id" : property.Name;
                magic.Add(name, new MagicProperty(property));
            }
            return magic;
        }        
    }

    internal class MagicProperty
    {
        private static readonly Type _myType = typeof(MagicProperty);
        private readonly PropertyInfo _property;
        
        public Type Type
        {
            get { return _property.PropertyType;  }
        }
        public string Name
        {
            get { return _property.Name; }
        }
        public Action<object, object> Setter { get; private set; }
        public Func<object, object> Getter { get; private set; }

        public MagicProperty(PropertyInfo property)
        {
            _property = property;
            Setter = CreateSetterMethod(property);
            Getter = CreateGetterMethod(property);
        }

        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }       
        private static Func<object, object> CreateGetterMethod(PropertyInfo method)
        {
            var genericHelper = _myType.GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);            
            var constructedHelper = genericHelper.MakeGenericMethod(method.DeclaringType, method.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { method });            
        }

        //called via reflection
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), method.GetSetMethod());
            return (target, param) => func((TTarget)target, (TParam)param);
        }
        private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {            
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), method.GetGetMethod());            
            return target => func((TTarget)target);            
        }
    }
}