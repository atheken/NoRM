
using System;
namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The kill cursors message.
    /// </summary>
    internal class KillCursorsMessage : Message
    {
        private long[] _killCursors;

        /// <summary>
        /// Initializes a new instance of the <see cref="KillCursorsMessage"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="fullyQualifiedCollName">The fully qualified coll name.</param>
        /// <param name="cursorsToKill">The cursors to kill.</param>
        internal KillCursorsMessage(IConnection connection, string fullyQualifiedCollName, params long[] cursorsToKill) : base(connection, fullyQualifiedCollName)
        {
            _killCursors = cursorsToKill;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}