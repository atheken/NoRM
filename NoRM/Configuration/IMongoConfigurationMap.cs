using System;

namespace Norm.Configuration
{
    /// <summary>
    /// Defines a configuration map
    /// </summary>
    public interface IMongoConfigurationMap : IHideObjectMembers
    {
        /// <summary>
        /// Configures a type's property mapping.
        /// </summary>
        /// <typeparam name="T">Object type under property mapping</typeparam>
        /// <param name="typeConfiguration">The type configuration.</param>
        void For<T>(Action<ITypeConfiguration<T>> typeConfiguration);

        /// <summary>
        /// Gets the name of the type's collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get collection name.</returns>
        string GetCollectionName(Type type);

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get connection string.</returns>
        string GetConnectionString(Type type);

        /// <summary>
        /// Gets the property alias for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the type's property.</param>
        /// <returns>
        /// Type's property alias if configured; otherwise null
        /// </returns>
        string GetPropertyAlias(Type type, string propertyName);
    }
}