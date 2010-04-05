using System.Collections.Generic;

namespace Norm.BSON
{
    using System;

    internal class ArrayWrapper<T> : BaseWrapper
    {
        private readonly List<T> _list = new List<T>();
        
        public override void Add(object value)
        {
            _list.Add((T) value);
        }

        protected override object CreateContainer(Type type, Type itemType)
        {
            return null;
        }

        protected override void SetContainer(object container)
        {
            if (container != null)
            {
                throw new MongoException("An container cannot exist when trying to deserialize an array");
            }
        }

        public override object Collection
        {
            get
            {
                return _list.ToArray();
            }
        }        
    }
}