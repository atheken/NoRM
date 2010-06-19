using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Responses;

namespace Norm.Protocol.Messages
{
    internal class FindAndModifyResult<T> : BaseStatusMessage
    {
        /// <summary>
        /// The result of the find and modify.
        /// </summary>
        public T Value { get; set; }
    }
}
