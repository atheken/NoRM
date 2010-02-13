using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands.Qualifiers
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
