using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Norm
{
    /// <summary>
    /// Values that can be used to define order direction.
    /// </summary>
    /// <remarks>
    /// This is in the Norm namespace because it makes for "easy access"
    /// </remarks>
    public enum OrderBy
    {
        /// <summary>
        /// Order Ascending
        /// </summary>
        Ascending = 1,
        /// <summary>
        /// Order Descending
        /// </summary>
        Descending = -1
    }
}
