namespace NoRM.BSON
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal static class ListHelper
    {
        private static readonly Type _IReadonlyGenericType = typeof(ReadOnlyCollection<>);

        public static Type GetListItemType(Type enumerableType)
        {
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType();
            }
            if (enumerableType.IsGenericType)
            {
                return enumerableType.GetGenericArguments()[0];
            }
            return typeof(object);
        }
        public static Array ToArray(List<object> container, Type itemType)
        {
            var array = Array.CreateInstance(itemType, container.Count);
            Array.Copy(container.ToArray(), 0, array, 0, container.Count);
            return array;
        }
        public static IList CreateContainer(Type listType, Type listItemType, out bool isReadOnly)
        {
            isReadOnly = false;
            if (listType.IsArray)
            {
                return new List<object>();
            }
            if (listType.IsInterface)
            {
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));
            }
            if (listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                return (IList)Activator.CreateInstance(listType);
            }
            if (_IReadonlyGenericType.IsAssignableFrom(listType.GetGenericTypeDefinition()))
            {
                isReadOnly = true;
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));
            }
            
            return new List<object>();
        }
    }
}