using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Commands.Qualifiers
{
    public class NotInQualifier<T>:QualifierCommand
    {
        public NotInQualifier(params T[] notInSet)
        {
            this.CommandName = "$nin";
            this.ValueForCommand = notInSet;
        }
    }
}
