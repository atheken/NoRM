using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class DeleteMessage<T> : Message where T : class, new()
    {
    }
}
