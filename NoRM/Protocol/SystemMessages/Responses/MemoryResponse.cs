using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Responses
{

    /// <summary>
    /// Memory response.
    /// </summary>
    public class MemoryResponse
    {
        public int? Resident { get; set; }
        public int? Virtual { get; set; }
        public long? Mapped { get; set; }
        public bool? Supported { get; set; }
    }
}
