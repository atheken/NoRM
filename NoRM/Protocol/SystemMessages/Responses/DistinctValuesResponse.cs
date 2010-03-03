using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Responses
{
    class DistinctValuesResponse<T> where T: class, new()
    {
        public List<T> Values { get; set; }
        public double? OK { get; set; }
    }
}
