using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class ProfileLevelResponse
    {
        /// <summary>
        /// Indicates that this response was valid.
        /// </summary>
        public double? OK { get; set; }
        /// <summary>
        /// Indicates the profile level.
        /// </summary>
        public double? Was { get; set; }
    }
}
