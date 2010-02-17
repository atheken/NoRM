using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers
{
    public class NotEqualQualifier : QualifierCommand
    {
        internal NotEqualQualifier(object value)
        {
            this.CommandName = "$ne";
            this.ValueForCommand = value;
        }
    }
}
