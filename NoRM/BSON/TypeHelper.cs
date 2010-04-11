using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Norm.Attributes;
using Norm.Configuration;

namespace Norm.BSON
{
    /// <summary>
    /// Convenience methods for type reflection.
    /// </summary>
    public class TypeHelper
    {
        private static readonly object _lock = new object();
        private static readonly IDictionary<Type, TypeHelper> _cachedTypeLookup = new Dictionary<Type, TypeHelper>();

        private static readonly Type _ignoredType = typeof(MongoIgnoreAttribute);
        private readonly IDictionary<string, MagicProperty> _properties;
        private readonly Type _type;

        static TypeHelper()
        {
            //register to be notified of type configuration changes.
            MongoConfiguration.TypeConfigurationChanged += new Action<Type>(TypeConfigurationChanged);
        }

        static void TypeConfigurationChanged(Type theType)
        {
            //clear the cache to prevent TypeHelper from getting the wrong thing.
            if (_cachedTypeLookup.ContainsKey(theType))
            {
                _cachedTypeLookup.Remove(theType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHelper"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TypeHelper(Type type)
        {
            _type = type;
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            _properties = LoadMagicProperties(properties, IdProperty(properties));
        }

        /// <summary>
        /// The get helper for type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static TypeHelper GetHelperForType(Type type)
        {
            TypeHelper helper;
            if (!_cachedTypeLookup.TryGetValue(type, out helper))
            {
                lock (_lock)
                {
                    if (!_cachedTypeLookup.TryGetValue(type, out helper))
                    {
                        helper = new TypeHelper(type);
                        _cachedTypeLookup[type] = helper;
                    }
                }
            }
            return helper;
        }

        /// <summary>
        /// Lifted from AutoMaper.  Finds a property using a lambda expression
        /// (i.e. x => x.Name)
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
                        var memberExpression = (MemberExpression)expressionToCheck;

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(
                                string.Format("Expression '{0}' must resolve to top-level member.", lambdaExpression),
                                "lambdaExpression");
                        }

                        return memberExpression.Member.Name;

                    default:
                        done = true;
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static PropertyInfo FindProperty(Type type, string name)
        {
            return type.GetProperties().Where(p => p.Name == name).First();
        }

        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <returns></returns>
        public ICollection<MagicProperty> GetProperties()
        {
            return _properties.Values;
        }

        /// <summary>
        /// Returns the magic property for the specified name, or null if it doesn't exist.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public MagicProperty FindProperty(string name)
        {
            return _properties.ContainsKey(name) ? _properties[name] : null;
        }

        /// <summary>
        /// Finds the id property.
        /// </summary>
        /// <returns></returns>
        public MagicProperty FindIdProperty()
        {
            return _properties.ContainsKey("$_id") ?
                _properties["$_id"] : _properties.ContainsKey("$id") ? _properties["$id"] : null;
        }

        /// <summary>
        /// Gets the id property.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        private static PropertyInfo IdProperty(IEnumerable<PropertyInfo> properties)
        {
            PropertyInfo foundSoFar = null;
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(BsonHelper.MongoIdentifierAttribute, true).Length > 0)
                {
                    return property;
                }
                if (property.Name.Equals("_id", StringComparison.InvariantCultureIgnoreCase))
                {
                    foundSoFar = property;
                }
                if (foundSoFar == null && property.Name.Equals("Id",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    foundSoFar = property;
                }
            }

            return foundSoFar;
        }

        /// <summary>
        /// Loads magic properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="idProperty">The id property.</param>
        /// <returns></returns>
        private static IDictionary<string, MagicProperty> LoadMagicProperties(IEnumerable<PropertyInfo> properties, PropertyInfo idProperty)
        {
            var magic = new Dictionary<string, MagicProperty>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(_ignoredType, true).Length > 0 ||
                    property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                //HACK: this is a latent BUG, if MongoConfiguration is altered after stashing the type helper, we die.
                var alias = MongoConfiguration.GetPropertyAlias(property.DeclaringType, property.Name);

                var name = (property == idProperty && alias != "$id") ? "$_id" : alias;
                magic.Add(name, new MagicProperty(property, property.DeclaringType));
            }

            return magic;
        }

        /// <summary>
        /// Determines the discriminator to use when serialising the type
        /// </summary>
        /// <returns></returns>
        public string GetTypeDiscriminator()
        {
            var discriminatingType = MongoDiscriminatedAttribute.GetDiscriminatingTypeFor(_type);
            if (discriminatingType != null)
            {
                return String.Join(",", _type.AssemblyQualifiedName.Split(','), 0, 2);
            }

            return null;
        }
    }
}
