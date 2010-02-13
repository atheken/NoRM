using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
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
