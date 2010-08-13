using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// A command that can be used as request.
    /// </summary>
    public class SliceQualifier : QualifierCommand
    {
        public SliceQualifier(int index)
            : this(index, index)
        {

        }

        public SliceQualifier(int left, int right)
            : base("$slice", new int[] { left, right })
        {

        }
    }
}
