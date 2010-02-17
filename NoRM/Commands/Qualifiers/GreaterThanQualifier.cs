using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    public class GreaterThanQualifier : QualifierCommand
    {
        internal GreaterThanQualifier(double value)
        {
            this.CommandName = "$gt";
            this.ValueForCommand = value;
        }
    }
}
