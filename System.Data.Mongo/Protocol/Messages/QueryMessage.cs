using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class QueryMessage<T> : Message where T : class, new()
    {
        public IEnumerable<T> Execute()
        {
            yield break;
        }
        
    }
}
