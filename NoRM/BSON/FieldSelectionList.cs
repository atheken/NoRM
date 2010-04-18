using System.Collections.Generic;

namespace Norm.BSON
{    
    internal class FieldSelectionList : List<string>
    {
        public FieldSelectionList(){}
        public FieldSelectionList(int capacity) : base(capacity){}
        public FieldSelectionList(IEnumerable<string> collection) : base(collection){}
    }
}