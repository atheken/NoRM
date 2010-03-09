using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NoRM.Attributes;
using System.Linq.Expressions;
using NoRM.BSON.DbTypes;
using NoRM.Configuration;

namespace NoRM.BSON
{
    internal class TypeHelper
    {
        private static readonly Type _ignoredType = typeof (MongoIgnoreAttribute);
        private readonly Type _type;
        private readonly IDictionary<string, MagicProperty> _properties;
        private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();
        
        public TypeHelper(Type type)
        {
            _type = type;
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
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

        /// <summary>
        /// Returns the magic property for the specified name, or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MagicProperty FindProperty(string name)
        {
            return this._properties.ContainsKey(name) ? this._properties[name] : null;
        }
        public MagicProperty FindIdProperty()
        {
            return _properties.ContainsKey("$_id") ? _properties["$_id"] : null;
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
                if (property.GetCustomAttributes(_ignoredType, true).Length > 0 || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var alias = MongoConfiguration.GetPropertyAlias(property.DeclaringType, property.Name);

                var name = (property == idProperty && alias != "$id")
                               ? "$_id"
                               : alias;
                magic.Add(name, new MagicProperty(property));
            }
            return magic;
        }

        /// <summary>
        /// Lifted from AutoMaper.
        /// </summary>
        /// <param name="lambdaExpression">The lambda expression.</param>
        /// <returns>Property name</returns>
        public static string FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            var done = false;

            while (!done)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;

                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = ((MemberExpression)expressionToCheck);

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter && memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression), "lambdaExpression");
                        }

                        return memberExpression.Member.Name;

                    default:
                        done = true;
                        break;
                }
            }

            return null;
        }

        public static PropertyInfo FindProperty(Type type, string name)
        {
            return type.GetProperties().Where(p => p.Name == name).First();
        }
    }

    internal class MagicProperty
    {
        private static readonly Type _myType = typeof(MagicProperty);
        private static readonly Type _ignoredIfNullType = typeof(MongoIgnoreIfNullAttribute);
        private readonly PropertyInfo _property;
        private readonly bool _ignoreIfNull;
        
        public Type Type
        {
            get { return _property.PropertyType;  }
        }
        public string Name
        {
            get { return _property.Name; }
        }
        public bool IgnoreIfNull
        {
            get { return _ignoreIfNull; }
        }

        public Action<object, object> Setter { get; private set; }
        public Func<object, object> Getter { get; private set; }

        public MagicProperty(PropertyInfo property)
        {
            _property = property;
            _ignoreIfNull = property.GetCustomAttributes(_ignoredIfNullType, true).Length > 0;
            Getter = CreateGetterMethod(property);
            Setter = CreateSetterMethod(property);
        }

        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }       
        private static Func<object, object> CreateGetterMethod(PropertyInfo property)
        {
            var genericHelper = _myType.GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { property });            
        }

        //called via reflection
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetSetMethod(true);
            if (m == null) { return null;  } //no setter
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
        }
        private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetGetMethod(true);
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);            
            return target => func((TTarget)target);            
        }
    }
}