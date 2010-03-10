using System;

namespace NoRM.Configuration
{
    /// <summary>
    /// Represents configuration mapping types names to database field names
    /// </summary>
    public class MongoConfigurationMap : IMongoConfigurationMap
    {

        /// <summary>
        /// Configures properties for type T
        /// </summary>
        /// <typeparam name="T">Type to configure</typeparam>
        /// <param name="typeConfigurationAction">The type configuration action.</param>
        public void For<T>(Action<ITypeConfiguration<T>> typeConfigurationAction)
        {
            var typeConfiguration = new MongoTypeConfiguration<T>();
            typeConfigurationAction((ITypeConfiguration<T>) typeConfiguration);
        }

        /// <summary>
        /// Gets the property alias for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the type's property.</param>
        /// <returns>
        /// Type's property alias if configured; otherwise null
        /// </returns>
        public string GetPropertyAlias(Type type, string propertyName)
        {
            var map = MongoTypeConfiguration.PropertyMaps;

            return map.ContainsKey(type) && map[type].ContainsKey(propertyName)
                       ? map[type][propertyName].Alias
                       : propertyName;
        }

        /// <summary>
        /// Gets the name of the type's collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get collection name.</returns>
        public string GetCollectionName(Type type)
        {
            var collections = MongoTypeConfiguration.CollectionNames;

            return collections.ContainsKey(type)
                       ? collections[type]
                       : type.Name;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get connection string.</returns>
        public string GetConnectionString(Type type)
        {
            var connections = MongoTypeConfiguration.ConnectionStrings;

            return connections.ContainsKey(type)
                       ? connections[type]
                       : null;
        }
    }
}