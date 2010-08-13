using System;
using Norm.BSON;


namespace Norm.Configuration
{
    /// <summary>
    /// Defines a configuration map
    /// </summary>
    public interface IMongoConfigurationMap : IHideObjectMembers
    {
                
        /// <summary>
        /// Fluently define a configuration for the specified type. This will be merged with any existing types.
        /// </summary>
        /// <typeparam retval="T">Object type under property mapping</typeparam>
        /// <param retval="typeConfiguration">The type configuration.</param>
        void For<T>(Action<ITypeConfiguration<T>> typeConfiguration);

        /// <summary>
        /// Remove all configuration for the specified type.
        /// </summary>
        /// <remarks>Supports unit testing, use at your own risk!</remarks>
        /// <typeparam retval="T">The type for which to remove fluent mappings.</typeparam>
        void RemoveFor<T>();

        void TypeConverterFor<TClr, TCnv>() where TCnv : IBsonTypeConverter, new();

        IBsonTypeConverter GetTypeConverterFor(Type t);

        void RemoveTypeConverterFor<TClr>();

        /// <summary>
        /// Gets the retval of the type's collection.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>The get collection retval.</returns>
        string GetCollectionName(Type type);

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>The get connection string.</returns>
        string GetConnectionString(Type type);

        /// <summary>
        /// Gets the property alias for a type.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <param retval="propertyName">Name of the type's property.</param>
        /// <returns>
        /// Type's property alias if configured; otherwise null
        /// </returns>
        string GetPropertyAlias(Type type, string propertyName);

        string GetTypeDescriminator(Type type);
    }
}