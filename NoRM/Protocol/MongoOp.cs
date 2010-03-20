
namespace Norm.Protocol
{
    /// <summary>
    /// The mongo op.
    /// </summary>
    public enum MongoOp
    {
        /// <summary>
        /// Reply to a client request. responseTo is set
        /// </summary>
        /// <remarks>
        /// Infrastructure. This is reserved for the database, 
        /// don't use it when creating a message. 
        /// </remarks>
        Reply = 1,

        /// <summary>
        /// generic msg command followed by a string
        /// </summary>
        Message = 1000,

        /// <summary>
        /// update document
        /// </summary>
        Update = 2001,

        /// <summary>
        /// Insert new document.
        /// </summary>
        Insert = 2002,

        /// <summary>
        /// Maybe NOT USED?
        /// </summary>
        GetByOID = 2003,

        /// <summary>
        /// Query a collection.
        /// </summary>
        Query = 2004,

        /// <summary>
        /// Get more data from a query.
        /// </summary>
        GetMore = 2005,

        /// <summary>
        /// Delete documents.
        /// </summary>
        Delete = 2006,

        /// <summary>
        /// Tell the database that the client is done with a cursor.
        /// </summary>
        KillCursors = 2007
    }
}