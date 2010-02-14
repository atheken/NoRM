using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
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
