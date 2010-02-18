using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages
{
    public enum ProfileLevel : int
    {
        ProfilingOff = 0,
        SlowOperations = 1,
        AllOperations = 2
    }
}
