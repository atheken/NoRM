
namespace NoRM.Configuration
{
    /// <summary>
    /// Defines a type's collection name and connection string.
    /// </summary>
    public interface ITypeConfiguration
    {
        /// <summary>
        /// Uses a collection name for a given type.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        void UseCollectionNamed(string collectionName);
        /// <summary>
        /// Uses a connection string for a given type.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        void UseConnectionString(string connectionString);
    }
}
