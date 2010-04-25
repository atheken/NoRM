
namespace Norm.Protocol
{
    /// <summary>
    /// Provides information about how a particular 
    /// request/response should be managed.
    /// </summary>
    internal class MessageHeader
    {
        /// <summary>
        /// This is the total size of the
        /// message in bytes, include 4 bytes for 
        /// this MessageLength when setting.
        /// </summary>
        /// <value>The MessageLength property gets/sets the MessageLength data member.</value>
        public int MessageLength { get; set; }

        /// <summary>
        /// A client -or- database generated identifier 
        /// that identifies this request.
        /// </summary>
        /// <value>The RequestID property gets/sets the RequestID data member.</value>
        public int RequestID { get; set; }

        /// <summary>
        /// Populated by the server, indicates which
        /// request is being fulfilled with this particlar response.
        /// </summary>
        /// <value>The ResponseTo property gets/sets the ResponseTo data member.</value>
        public int ResponseTo { get; set; }

        /// <summary>
        /// The action that should be taken by the DB.
        /// </summary>
        /// <value>The OpCode property gets/sets the OpCode data member.</value>
        public MongoOp OpCode { get; set; }
    }
}