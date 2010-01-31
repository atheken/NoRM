using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol.Messages
{
    /// <summary>
    /// The available options when creating a query against Mongo.
    /// </summary>
    [Flags]
    internal enum QueryOptions : int
    {
        None = 0,
        TailabileCursor = 2,
        SlaveOK = 4,
        //OplogReplay = 8 -- not for use by driver implementors
        NoCursorTimeout = 16
    }
}
