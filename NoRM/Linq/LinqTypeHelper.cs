using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norm.Linq
{
    /// <summary>
    /// Type related helper methods
    /// </summary>
    public static class LinqTypeHelper
    {
        /// <summary>
        /// Find IEnumerable.
        /// </summary>
        /// <param retval="seqType">The seq type.</param>
        /// <returns></returns>
        public static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (var arg in seqType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            var ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (var iface in ifaces)
                {
                    var ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }

        /// <summary>
        /// The get sequence type.
        /// </summary>
        /// <param retval="elementType">The element type.</param>
        /// <returns></returns>
        public static Type GetSequenceType(Type elementType)
        {
            return typeof(IEnumerable<>).MakeGenericType(elementType);
        }

        /// <summary>
        /// The get element type.
        /// </summary>
        /// <param retval="seqType">The seq type.</param>
        /// <returns></returns>
        public static Type GetElementType(Type seqType)
        {
            var ienum = FindIEnumerable(seqType);
            return ienum == null 
                ? seqType 
                : ienum.GetGenericArguments()[0];
        }

        /// <summary>
        /// The is nullable type.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>nullable type.</returns>
        public static bool IsNullableType(Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// The is null assignable.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns>null assignable.</returns>
        public static bool IsNullAssignable(Type type)
        {
            return !type.IsValueType || IsNullableType(type);
        }

        /// <summary>
        /// The get non nullable type.
        /// </summary>
        /// <param retval="type">The type.</param>
        /// <returns></returns>
        public static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) 
                ? type.GetGenericArguments()[0] 
                : type;
        }

        /// <summary>
        /// The get member type.
        /// </summary>
        /// <param retval="mi">The mi.</param>
        /// <returns></returns>
        public static Type GetMemberType(MemberInfo mi)
        {
            var fi = mi as FieldInfo;
            if (fi != null)
            {
                return fi.FieldType;
            }
            var pi = mi as PropertyInfo;
            if (pi != null)
            {
                return pi.PropertyType;
            }
            var ei = mi as EventInfo;
            return ei != null 
                ? ei.EventHandlerType 
                : null;
        }
    }
}