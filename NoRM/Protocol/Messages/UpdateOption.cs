using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.Messages
{
    [Flags]
    internal enum UpdateOption : int
    {
        None = 0,
        Upsert = 1,
        MultiUpdate = 2
    }
}
