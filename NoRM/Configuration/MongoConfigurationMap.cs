using System;
using Norm.BSON;
using System.Collections.Generic;
using System.Reflection;
using Norm.BSON.DbTypes;

namespace Norm.Configuration
{
    /// <summary>
    /// Represents configuration mapping types names to database field names
    /// </summary>
    public class MongoConfigurationMap : IMongoConfigurationMap
    {
        private Dictionary<Type, String> _idProperties = new Dictionary<Type, string>();

        /// <summary>
        /// Configures properties for type T
        /// </summary>
        /// <typeparam name="T">Type to configure</typeparam>
        /// <param name="typeConfigurationAction">The type configuration action.</param>
        public void For<T>(Action<ITypeConfiguration<T>> typeConfigurationAction)
        {
            var typeConfiguration = new MongoTypeConfiguration<T>();
            typeConfigurationAction((ITypeConfiguration<T>)typeConfiguration);
        }

        private bool IsIdPropertyForType(Type type, String propertyName)
        {
            bool retval = false;

            if (!_idProperties.ContainsKey(type))
            {
                PropertyInfo idProp = TypeHelper.FindIdProperty(type);

                if (idProp != null)
                {
                    _idProperties[type] = idProp.Name;
                    retval = idProp.Name == propertyName;
                }
            }
            else
            {
                retval = _idProperties[type] == propertyName;
            }
            return retval;
        }

        /// <summary>
        /// Checks to see if the object is a DbReference. If it is, we won't want to override $id to _id.
        /// </summary>
        /// <param name="type">The type of the object being serialized.</param>
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
        /// Gets the property alias for a type.
        /// </summary>
        /// <remarks>
        /// If it's the ID Property, returns "_id" regardless of additional mapping.
        /// If it's not the ID Property, returns the mapped name if it exists.
        /// Else return the original propertyName.
        /// </remarks>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the type's property.</param>
        /// <returns>
        /// Type's property alias if configured; otherwise null
        /// </returns>
        public string GetPropertyAlias(Type type, string propertyName)
        {
            var map = MongoTypeConfiguration.PropertyMaps;
            var retval = propertyName;//default to the original.

            if (IsIdPropertyForType(type, propertyName) && !IsDbReference(type))
            {
                retval = "_id";
            }
            else if (map.ContainsKey(type))
            {
                foreach (var m in map.Keys)
                {
                    if (map[m].ContainsKey(propertyName))
                    {
                        retval = map[m][propertyName].Alias;
                        break;
                    }
                }
            }
            return retval;
        }

        public Type SummaryTypeFor(Type type)
        {
            var summaryTypes = MongoTypeConfiguration.SummaryTypes;
            return summaryTypes.ContainsKey(type) ? summaryTypes[type] : null;
        }

        /// <summary>
        /// Gets the name of the type's collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get collection name.</returns>
        public string GetCollectionName(Type type)
        {
            var collections = MongoTypeConfiguration.CollectionNames;
            return collections.ContainsKey(type) ? collections[type] : type.Name;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get connection string.</returns>
        public string GetConnectionString(Type type)
        {
            var connections = MongoTypeConfiguration.ConnectionStrings;
            return connections.ContainsKey(type) ? connections[type] : null;
        }

        /// <summary>
        /// Removes the mapping for this type.
        /// </summary>
        /// <remarks>
        /// Added to support Unit testing. Use at your own risk!
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        public void RemoveFor<T>()
        {
            MongoTypeConfiguration.RemoveMappings<T>();
        }

    }
}