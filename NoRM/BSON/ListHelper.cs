using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NoRM.BSON
{
    /// <summary>
    /// The list helper.
    /// </summary>
    internal static class ListHelper
    {
        private static readonly Type _IReadonlyGenericType = typeof(ReadOnlyCollection<>);

        /// <summary>
        /// The get list item type.
        /// </summary>
        /// <param name="enumerableType">The enumerable type.</param>
        /// <returns></returns>
        public static Type GetListItemType(Type enumerableType)
        {
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType();
            }

            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[0]
                : typeof(object);
        }

        /// <summary>
        /// The get dictionar key type.
        /// </summary>
        /// <param name="enumerableType">The enumerable type.</param>
        /// <returns></returns>
        public static Type GetDictionarKeyType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[0]
                : typeof(object);
        }

        /// <summary>
        /// The get dictionar value type.
        /// </summary>
        /// <param name="enumerableType">The enumerable type.</param>
        /// <returns></returns>
        public static Type GetDictionarValueType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[1]
                : typeof(object);
        }

        /// <summary>
        /// Converts a list to an array
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="itemType">The item type.</param>
        /// <returns></returns>
        public static Array ToArray(List<object> container, Type itemType)
        {
            var array = Array.CreateInstance(itemType, container.Count);
            Array.Copy(container.ToArray(), 0, array, 0, container.Count);
            return array;
        }

        /// <summary>
        /// The create container.
        /// </summary>
        /// <param name="listType">The list type.</param>
        /// <param name="listItemType">The list item type.</param>
        /// <param name="isReadOnly">The is read only.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a dictionary.
        /// </summary>
        /// <param name="dictionaryType">The dictionary type.</param>
        /// <param name="keyType">The key type.</param>
        /// <param name="valueType">The value type.</param>
        /// <returns></returns>
        public static IDictionary CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            if (dictionaryType.IsInterface)
            {
                return (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            }

            if (dictionaryType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                return (IDictionary)Activator.CreateInstance(dictionaryType);
            }

            return new Dictionary<object, object>();
        }
    }
}