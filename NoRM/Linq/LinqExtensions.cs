using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NoRM.BSON;

namespace NoRM.Linq {
    public static class LinqExtensions {
        /// <summary>
        /// Gets the constant value.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <returns></returns>
        public static object GetConstantValue(this Expression exp) {
            object result = null;
            if (exp is ConstantExpression) {
                ConstantExpression c = (ConstantExpression)exp;
                result = c.Value;
            }
            return result;
        }
    }
}
