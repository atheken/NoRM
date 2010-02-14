using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
{
    public class AllQualifier<T> : QualifierCommand
    {
        public AllQualifier(params T[] all)
        {
            this.CommandName = "$all";
            this.ValueForCommand = all;
        }
    }
}
