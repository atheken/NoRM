using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSONLib
{
    public abstract class ModifierOperation
    {
        public virtual String CommandName { get; protected set; }
        public virtual Object ValueForCommand { get; set; }
    }
}
