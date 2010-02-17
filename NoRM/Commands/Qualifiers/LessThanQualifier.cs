using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    public class LessThanQualifier : QualifierCommand
    {
        /// <summary>
        /// Builds a less than qualifier, really ought to be a number (MAYBE a string)
        /// </summary>
        /// <param name="value"></param>
        internal LessThanQualifier(Object value)
        {
            this.CommandName = "$lt";
            this.ValueForCommand = value;
        }
       
    }
}
