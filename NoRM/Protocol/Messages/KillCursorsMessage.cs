using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.Messages
{
    internal class KillCursorsMessage : Message
    {
        private long[] _killCursors;
        internal KillCursorsMessage(MongoServer context, String fullyQualifiedCollName, params long[] cursorsToKill)
            : base(context, fullyQualifiedCollName)
        {
            this._killCursors = cursorsToKill;
        }
    }
}
