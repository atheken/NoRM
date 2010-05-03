using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    public class ElementMatch<T> : QualifierCommand
    {
        public ElementMatch(T matchDoc)
            : base("$elemMatch", matchDoc)
        {

        }
    }
}
