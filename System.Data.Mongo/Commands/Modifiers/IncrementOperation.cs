using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BSONLib;

namespace System.Data.Mongo.Commands
{
    public class IncrementOperation : ModifierCommand
    {
        public IncrementOperation(int amountToIncrement)
        {
            this.CommandName = "$inc";
            this.ValueForCommand = amountToIncrement;
        }
    }
}
