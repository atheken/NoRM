using Norm.BSON;

namespace Norm.Responses
{
    /// <summary>
    /// The dropped database response.
    /// </summary>
    public class DroppedDatabaseResponse : BaseStatusMessage
    {
        /// <summary>
        /// Gets or sets the dropped database.
        /// </summary>
        /// <value>The dropped.</value>
        public string Dropped { get; set; }
    }
}