using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
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
