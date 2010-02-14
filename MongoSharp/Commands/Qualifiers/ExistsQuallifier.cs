using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Commands.Qualifiers
{
    public class ExistsQuallifier : QualifierCommand
    {
        internal ExistsQuallifier(bool doesExist)
        {
            this.CommandName = "$exists";
            this.ValueForCommand = doesExist;
        }
    }
}
