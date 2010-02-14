using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
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
