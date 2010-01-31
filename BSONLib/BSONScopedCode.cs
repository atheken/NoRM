using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSONLib
{
    /// <summary>
    /// A class that represents code with scoping - will be serialized to 
    /// </summary>
    public class BSONScopedCode
    {
        /// <summary>
        /// The scope code.
        /// </summary>
        public String CodeString { get; set; }
        
        //would be useful to add implicit conversion to/from string
    }
}
