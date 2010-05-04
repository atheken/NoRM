using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Norm.BSON
{
    internal static class ListHelper
    {
        public static Type GetListItemType(Type enumerableType)
        {
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType();
            }
            return enumerableType.IsGenericType ? enumerableType.GetGenericArguments()[0] : typeof(object);
        }

        public static Type GetDictionarKeyType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[0]
                : typeof(object);
        }

        public static Type GetDictionarValueType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[1]
                : typeof(object);
        }

        public static IDictionary CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            IDictionary retval = new Dictionary<object, object>(0);
            if (dictionaryType.IsInterface)
            {
                retval = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            }
            else if (dictionaryType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                retval = (IDictionary)Activator.CreateInstance(dictionaryType);
            }
            return retval;
        }
    }
}