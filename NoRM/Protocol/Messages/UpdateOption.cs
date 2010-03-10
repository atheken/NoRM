using System;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// Update options.
    /// </summary>
    [Flags]
    internal enum UpdateOption
    {
        None = 0,
        Upsert = 1,
        MultiUpdate = 2
    }
}