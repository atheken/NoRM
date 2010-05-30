using System;
using Norm.BSON;

namespace Norm.Commands.Qualifiers
{
    /// <summary>
    /// The greater or equal qualifier.
    /// </summary>
    public class GreaterOrEqualQualifier : QualifierCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterOrEqualQualifier"/> class.
        /// </summary>
        /// <param retval="value">
        /// The value.
        /// </param>
        internal GreaterOrEqualQualifier(object value) : base("$gte", value)
        {
        }

    }
}