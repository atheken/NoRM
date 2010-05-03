using System.Collections.Generic;

namespace Norm.BSON
{    
    internal class FieldSelectionList : List<string>
    {
        public FieldSelectionList(int capacity) : base(capacity){}
    }
}