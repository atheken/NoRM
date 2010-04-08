using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Norm.BSON
{
    internal abstract class BaseWrapper
    {
        public static BaseWrapper Create(Type type, Type itemType, object existingContainer)
        {
            var instance = CreateWrapperFromType(existingContainer == null ? type : existingContainer.GetType(), itemType);
            instance.SetContainer(existingContainer ?? instance.CreateContainer(type, itemType));
            return instance;
        }

        private static BaseWrapper CreateWrapperFromType(Type type, Type itemType)
        {
            BaseWrapper retval = null;
            if (type.IsArray)
            {
                retval = (BaseWrapper)Activator.CreateInstance(typeof(ArrayWrapper<>).MakeGenericType(itemType));
            }
            else
            {
                var types = new List<Type>(type.GetInterfaces()
                    .Select(h => h.IsGenericType ? h.GetGenericTypeDefinition() : h));
                types.Insert(0, type.IsGenericType ? type.GetGenericTypeDefinition() : type);

                if (types.Any(i => typeof(IList<>).IsAssignableFrom(i) || typeof(IList).IsAssignableFrom(i)))
                {
                    retval = new ListWrapper();
                }
                else if (types.Any(y => typeof(ICollection<>).IsAssignableFrom(y)))
                {
                    retval = (BaseWrapper)Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(itemType));
                }
                else if (types.Any(i => typeof(IEnumerable<>).IsAssignableFrom(i) || typeof(IEnumerable).IsAssignableFrom(i)))
                {
                    retval = new ListWrapper();
                }
                else if (retval == null)
                {
                    //we gave it our best shot, but we couldn't figure out how to deserialize this badboy.
                    throw new MongoException(string.Format("Collection of type {0} cannot be deserialized.", type.FullName));
                }
            }
            return retval;
        }

        public abstract void Add(object value);
        public abstract object Collection { get; }

        protected abstract object CreateContainer(Type type, Type itemType);
        protected abstract void SetContainer(object container);
    }
}