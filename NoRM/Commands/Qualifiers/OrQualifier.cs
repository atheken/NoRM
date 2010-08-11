using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.BSON;

namespace Norm.Commands
{
    public class OrQualifier : QualifierCommand
    {
        public OrQualifier(params Object[] orCriteriaGroups) :
            base("$or", orCriteriaGroups)
        {
        }
    }
}
