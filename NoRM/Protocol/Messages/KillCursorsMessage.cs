namespace NoRM.Protocol.Messages
{
    internal class KillCursorsMessage : Message
    {
        private long[] _killCursors;
        
        internal KillCursorsMessage(IConnection connection, string fullyQualifiedCollName, params long[] cursorsToKill) : base(connection, fullyQualifiedCollName)
        {
            _killCursors = cursorsToKill;
        }
    }
}
