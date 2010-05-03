using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Norm.BSON
{
    internal class ListWrapper : BaseWrapper
    {
        private IList _list;

        public override object Collection
        {
            get { return _list; }
        }
        public override void Add(object value)
        {
            _list.Add(value);
        }

        protected override object CreateContainer(Type type, Type itemType)
        {
            object retval = null;
            if (type.IsInterface)
            {
                retval = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            }
            else if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                retval = Activator.CreateInstance(type);
            }
            return retval;
        }
        protected override void SetContainer(object container)
        {
            _list = container == null ? new ArrayList() : (IList)container;
        }
    }
}