using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.BSON
{
    public abstract class ModifierCommand
    {
        public virtual String CommandName { get; protected set; }
        public virtual Object ValueForCommand { get; set; }
    }
    public abstract class QualifierCommand
    {
        public virtual String CommandName { get; protected set; }
        public virtual Object ValueForCommand { get; set; }
    }
}
