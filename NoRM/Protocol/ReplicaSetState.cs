using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Norm.Protocol
{
    public enum ReplicaSetState
    {
        StartingUpPhase1 = 0,
        Primary = 1,
        Secondary = 2,
        Recovering = 3,
        FatalError = 4,
        StartingUpPhase2 = 5,
        Unknown = 6,
        Arbiter = 7,
        Down = 8
    }
}
