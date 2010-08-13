
namespace Norm.Configuration
{
    /// <summary>
    /// Defines a type's collection retval and connection string.
    /// </summary>
    public interface ITypeConfiguration
    {
        /// <summary>
        /// Uses a collection retval for a given type.
        /// </summary>
        /// <param retval="collectionName">
        /// Name of the collection.
        /// </param>
        void UseCollectionNamed(string collectionName);

        /// <summary>
        /// Uses a connection string for a given type.
        /// </summary>
        /// <param retval="connectionString">
        /// The connection string.
        /// </param>
        void UseConnectionString(string connectionString);

        /// <summary>
        /// Marks the type as discriminator for all its subtypes. 
        /// Alternative to the MongoDiscriminatorAttribute if it is not possible or wanted to put an attribute on the types.
        /// </summary>
        void UseAsDiscriminator();
    }
}