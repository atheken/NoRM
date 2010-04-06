using System;
using System.Collections.Generic;

namespace Norm.BSON
{
    internal class CollectionWrapper<T> : BaseWrapper
    {
        private ICollection<T> _list;

        public override object Collection
        {
            get { return _list; }
        }

        public override void Add(object value)
        {
            _list.Add((T)value);
        }

        protected override object CreateContainer(Type type, Type itemType)
        {
            return Activator.CreateInstance(type);
        }
        protected override void SetContainer(object container)
        {            
            _list = (ICollection<T>)container;
        }
    }   
}