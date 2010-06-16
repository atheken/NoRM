using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Norm.Attributes;
using Norm.Configuration;
using System.Text.RegularExpressions;

namespace Norm.BSON
{
    /// <summary>
    /// Convenience methods for type reflection.
    /// </summary>
    /// <remarks>
    /// This was formerly "Norm.BSON.TypeHelper" but the name was in conflict with a BCL type, so it has been changed to "ReflectionHelper"
    /// </remarks>
    public class ReflectionHelper
    {
        private static readonly object _lock = new object();
        private static readonly IDictionary<Type, ReflectionHelper> _cachedTypeLookup = new Dictionary<Type, ReflectionHelper>();

        private static readonly Type _ignoredType = typeof(MongoIgnoreAttribute);
        private readonly IDictionary<string, MagicProperty> _properties;
        private readonly Type _type;

        static ReflectionHelper()
        {
            //register to be notified of type configuration changes.
            MongoConfiguration.TypeConfigurationChanged += new Action<Type>(TypeConfigurationChanged);
        }

        static void TypeConfigurationChanged(Type theType)
        {
            //clear the cache to prevent ReflectionHelper from getting the wrong thing.
            if (_cachedTypeLookup.ContainsKey(theType))
            {
                _cachedTypeLookup.Remove(theType);
            }
        }

        /// <summary>
        /// A regex that gets everything up tot the first backtick, useful when searching for a good starting name.
        /// </summary>
        private static readonly Regex _rxGenericTypeNameFinder = new Regex("[^`]+", RegexOptions.Compiled);

        /// <summary>
        /// Given a type, this will produce a mongodb save version of the name, for example:
        /// 
        /// Product&lt;UKSupplier&gt; will become "Product_UKSupplier" - this is helpful for generic typed collections.
        /// </summary>
        /// <param name="t"></param>
        public static string GetScrubbedGenericName(Type t)
        {
            String retval = t.Name;
            if (t.IsGenericType)
            {
                retval = _rxGenericTypeNameFinder.Match(t.Name).Value;
                foreach (var a in t.GetGenericArguments())
                {
                    retval += "_" + GetScrubbedGenericName(a);
                }
            }
            return retval;
        }

        /// <summary>
        /// Returns the PropertyInfo for properties defined as Instance, Public, NonPublic, or FlattenHierarchy
        /// </summary>
        /// <param retval="type">The type.</param>
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        public static PropertyInfo[] GetInterfaceProperties(Type type)
        {
            List<PropertyInfo> interfaceProperties;
            Type[] interfaces = type.GetInterfaces();

            if (interfaces.Count() != 0)
            {
                interfaceProperties = new List<PropertyInfo>();
                foreach (Type nextInterface in interfaces)
                {
                    PropertyInfo[] intProps = GetProperties(nextInterface);
                    interfaceProperties.AddRange(intProps);
                }
                return interfaceProperties.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHelper"/> class.
        /// </summary>
        /// <param retval="type">The type.</param>
        public ReflectionHelper(Type type)
        {
            _type = type;
            var properties = GetProperties(type);
            _properties = LoadMagicProperties(properties, new IdPropertyFinder(type, properties).IdProperty);

            if (typeof(IExpando).IsAssignableFrom(type))
            {
                this.IsExpando = true;
            }
        }

        /// <summary>
        /// The get helper for type.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns></returns>
        public static ReflectionHelper GetHelperForType(Type type)
        {
            ReflectionHelper helper;
            if (!_cachedTypeLookup.TryGetValue(type, out helper))
            {
                lock (_lock)
                {
                    if (!_cachedTypeLookup.TryGetValue(type, out helper))
                    {
                        helper = new ReflectionHelper(type);
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
        /// <param retval="lambdaExpression">The lambda expression.</param>
        /// <returns>Property retval</returns>
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
        /// <param retval="type">The type.</param>
        /// <param retval="retval">The retval.</param>
        /// <returns></returns>
        public static PropertyInfo FindProperty(Type type, string name)
        {
            return type.GetProperties().Where(p => p.Name == name).First();
        }

        /// <summary>
        /// indicates if this type implements "IExpando"
        /// </summary>
        public bool IsExpando { get; private set; }
        
        /// <summary>
        /// Gets all properties.
        /// </summary>
        /// <returns></returns>
        public ICollection<MagicProperty> GetProperties()
        {
            return _properties.Values;
        }

        /// <summary>
        /// Returns the magic property for the specified retval, or null if it doesn't exist.
        /// </summary>
        /// <param retval="retval">The retval.</param>
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

        ///<summary>
        /// Returns the property defined as the Id for the entity either by convention or explicitly. 
        ///</summary>
        ///<param retval="type">The type.</param>
        ///<returns></returns>
        public static PropertyInfo FindIdProperty(Type type)
        {
            return new IdPropertyFinder(type).IdProperty;
        }

        /// <summary>
        /// Loads magic properties.
        /// </summary>
        /// <param retval="properties">The properties.</param>
        /// <param retval="idProperty">The id property.</param>
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

        /// <summary>
        /// Apply default values to the properties in the instance
        /// </summary>
        /// <param retval="typeHelper"></param>
        /// <param retval="instance"></param>
        public void ApplyDefaultValues(object instance)
        {
            // error check.
            if (instance != null)
            {
                // get all the properties
                foreach (var prop in this.GetProperties())
                {
                    // see if the property has a DefaultValue attribute
                    if (prop.HasDefaultValue)
                    {
                        // set the default value for the property.
                        prop.Setter(instance, prop.GetDefaultValue());
                    }
                }
            }
        }

    }
}
