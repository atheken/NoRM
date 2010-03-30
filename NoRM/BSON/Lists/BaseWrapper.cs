using System;
using System.Collections;
using System.Collections.Generic;

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
            if (type.IsArray)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(ArrayWrapper<>).MakeGenericType(itemType));
            }

            var isCollection = false;            
            var types = new List<Type>(type.GetInterfaces());
            types.Insert(0, type);            
            foreach(var @interface in types)
            {
                var interfaceType = @interface.IsGenericType ? @interface.GetGenericTypeDefinition() : @interface;
                if (typeof(IList<>).IsAssignableFrom(interfaceType) || typeof(IList).IsAssignableFrom(interfaceType))
                {
                    return new ListWrapper();
                }
                if (typeof(ICollection<>).IsAssignableFrom(interfaceType))
                {
                    isCollection = true;
                }
            }
            if (isCollection)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(itemType));
            }
            throw new MongoException(string.Format("Collection of type {0} cannot be deserialized", type.FullName));
        }

        public abstract void Add(object value);
        public abstract object Collection { get; }

        protected abstract object CreateContainer(Type type, Type itemType);
        protected abstract void SetContainer(object container);        
    }      
}