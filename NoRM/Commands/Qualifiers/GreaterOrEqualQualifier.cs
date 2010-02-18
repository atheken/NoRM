using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    public class GreaterOrEqualQualifier : QualifierCommand
    {
        internal GreaterOrEqualQualifier(double value)
        {
            this.CommandName = "$gte";
            this.ValueForCommand = value;
        }
    }
}
