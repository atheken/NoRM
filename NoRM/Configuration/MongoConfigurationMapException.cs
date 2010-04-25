namespace Norm.Configuration
{
    ///<summary>
    ///</summary>
    public class MongoConfigurationMapException : MongoException
    {
        ///<summary>
        /// Defines problems with type mappings.
        ///</summary>
        ///<param name="message"></param>
        public MongoConfigurationMapException(string message)
            : base(message)
        {}
    }
}