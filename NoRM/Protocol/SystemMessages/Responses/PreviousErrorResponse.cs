using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Responses
{
    public class PreviousErrorResponse
    {

        /// <summary>
        /// Whether this response is "ok"
        /// </summary>
        public double? OK { get; set; }
        
        /// <summary>
        /// Number of errors.
        /// </summary>
        public long? N { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public String Err { get; set; }

        /// <summary>
        /// Number of operations since these errors happened.
        /// </summary>
        public long? NPrev { get; set; }
    }
}
