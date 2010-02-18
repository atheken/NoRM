using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    public class SizeQualifier : QualifierCommand
    {
        internal SizeQualifier(double value)
        {
            this.CommandName = "$size";
            this.ValueForCommand = value;
        }
    }
}
