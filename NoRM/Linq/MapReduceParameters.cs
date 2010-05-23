using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Norm.Linq
{
    /// <summary>
    /// Javascript functions used to perform a map/reduce
    /// </summary>
    public class MapReduceParameters
    {
        public string Map { get; set; }
        public string Reduce { get; set; }
        public string Finalize { get; set; }
    }
}
