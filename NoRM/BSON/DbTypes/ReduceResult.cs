using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.BSON.DbTypes
{
    /// <summary>
    /// The agglomeration of the aggregation key, and the result of aggregation.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class ReduceResult<K,R>
    {
        public K Key { get; set; }
        public R Reduction { get; set; }
    }
}
