using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Commands.Qualifiers
{
    public class InQualifier<T> :QualifierCommand
    {
        public InQualifier(params T[] inset)
        {
            this.CommandName = "$in";
            this.ValueForCommand = inset;
        }
    }
}
