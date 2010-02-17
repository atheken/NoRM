using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Commands.Qualifiers
{
    public class LessOrEqualQualifier: QualifierCommand
    {
        internal LessOrEqualQualifier(double value)
        {
            this.CommandName = "$lte";
            this.ValueForCommand = value;
        }
    }
}
