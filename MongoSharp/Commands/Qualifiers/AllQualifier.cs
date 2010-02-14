using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Commands.Qualifiers
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
