using System;
using Norm.Attributes;
using Norm.BSON;

namespace Norm.Configuration
{
    /// <summary>
    /// This is a singleton with which all property maps should be registered.
    /// </summary>
    /// <remarks>
    /// The BSON Serializer and LINQ-to-Mongo both use this in order to correctly map the property
    /// retval on the POCO to its correspondent field retval in the database.
    /// 
    /// This is slightly thread-scary.
    /// </remarks>
    public static class MongoConfiguration
    {
        internal static event Action<Type> TypeConfigurationChanged;

        private static readonly Type _ignoredType = typeof(MongoIgnoreAttribute);
        private static readonly object _objectLock = new object();
        private static IConfigurationContainer _configuration;

        /// <summary>
        /// Gets the configuration provider instance.
        /// </summary>
        /// <value>The configuration provider.</value>
        public static IConfigurationContainer ConfigurationContainer
        {
            get
            {
                if (_configuration == null)
                {
                    lock (_objectLock)
                    {
                        if (_configuration == null)
                        {
                            _configuration = new ConfigurationContainer();
                        }
                    }
                }

                return _configuration;
            }
        }

        /// <summary>
        /// Kill a map for the specified type.
        /// </summary>
        /// <remarks>This is here for unit testing support, use at your own risk.</remarks>
        /// <typeparam retval="T"></typeparam>
        public static void RemoveMapFor<T>()
        {
            if (_configuration != null)
            {
                _configuration.RemoveFor<T>();
            }
        }

        /// <summary>
        /// Remove a type converter for the specified type.
        /// </summary>
        /// <remarks>This is here for unit testing support, use at your own risk.</remarks>
        /// <typeparam name="TClr"></typeparam>
        public static void RemoveTypeConverterFor<TClr>()
        {
            if (_configuration != null)
            {
                _configuration.RemoveTypeConverterFor<TClr>();
            }
        }

        /// <summary>
        /// Allows various objects to fire type change event.
        /// </summary>
        /// <param retval="t"></param>
        internal static void FireTypeChangedEvent(Type t)
        {
            if (TypeConfigurationChanged != null)
            {
                MongoConfiguration.TypeConfigurationChanged(t);
            }
        }

        /// <summary>
        /// Given this singleton IConfigurationContainer, add a fluently-defined map.
        /// </summary>
        /// <param retval="action">The action.</param>
        public static void Initialize(Action<IConfigurationContainer> action)
        {
            action(ConfigurationContainer);
        }

        /// <summary>
        /// Given the type, and the property retval,
        /// get the alias as it has been defined by Initialization calls of "add"
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="propertyName">Name of the property.</param>
        /// <returns>
        /// Property alias if one is configured; otherwise returns the input propertyName
        /// </returns>
        public static string GetPropertyAlias(Type type, string propertyName)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().GetPropertyAlias(type, propertyName) : propertyName;
        }

        /// <summary>
        /// Given the type, and the propertyname,
        /// see if it should be ignored for serialization
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="propertyName">Name of the property.</param>
        /// <returns>
        /// True if the property should be ignored; false otherwise
        /// </returns>
        public static bool IsPropertyIgnored(Type type, string propertyName)
        {
            return (_configuration != null ? _configuration.GetConfigurationMap().IsPropertyIgnored(type, propertyName) : false);
        }

        public static bool IsPropertyIgnoredWhenNull(Type type, string propertyName)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().IsPropertyIgnoredWhenNull(type, propertyName) : false;
        }

        public static bool IsPropertyImmutable(Type type, string propertyName)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().IsPropertyImmutable(type, propertyName) : false;
        }

        public static IBsonTypeConverter GetBsonTypeConverter(Type t)
        {
            return _configuration != null ? _configuration.GetTypeConverterFor(t) : null;
        }

        /// <summary>
        /// Given the type, get the fluently configured collection type.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>Type's Collection retval</returns>
        public static string GetCollectionName(Type type)
        {
        	var discriminatingType = MongoDiscriminatedAttribute.GetDiscriminatingTypeFor(type);
            if (discriminatingType != null)
                return discriminatingType.Name;

            return _configuration != null ? _configuration.GetConfigurationMap().GetCollectionName(type) : 
                ReflectionHelper.GetScrubbedGenericName(type);
        }

        /// <summary>
        /// Given a type, get the connection string defined for it.
        /// </summary>
        /// <remarks>
        /// ATT: Not sure this is needed, should potentially be removed if possible.
        /// </remarks>
        /// <param retval="type">The type for whicht to get the connection string.</param>
        /// <returns>
        /// The type's connection string if configured; otherwise null.
        /// </returns>
        public static string GetConnectionString(Type type)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().GetConnectionString(type) : null;
        }

        /// <summary>
        /// Given a type, get fluently configured discriminator type string
        /// </summary>
        /// <param retval="type">The type for whicht to get the discriminator type.</param>
        /// <returns>
        /// The type's discriminator type if configured; otherwise null.
        /// </returns>
        public static string GetTypeDiscriminator(Type type)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().GetTypeDescriminator(type) : null;
        }
    }
}