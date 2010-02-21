using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Commands.Qualifiers {
    public class WhereQualifier : QualifierCommand {
        public WhereQualifier(string inExpression) {
            this.CommandName = "$where";
            this.ValueForCommand = inExpression;
        }
    }
}
