using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MongoSharp
{
    public static class ExpandoProps
    {
        private static Dictionary<WeakReference, IDictionary<String, object>> _expandoProps =
        new Dictionary<WeakReference, IDictionary<string, object>>(0);
        private static ReaderWriterLockSlim _dictionaryLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static String _lockToken = "LOCK_THREAD";
        private static Thread _scrubExpandos;

        /// <summary>
        /// Set the deserialized props for the specified object into a global cache, to be cleared every 30 seconds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="addedProps"></param>
        internal static void SetPropsForObject<T>(T obj, IDictionary<String, object> addedProps, MongoServer context)
        {
            if (context.EnableExpandoProperties == true)
            {
                #region Initialize expandos dictionary and clean-up thread.
                if (ExpandoProps._scrubExpandos == null)
                {
                    lock (ExpandoProps._lockToken)
                    {
                        if (ExpandoProps._scrubExpandos == null)
                        {
                            ExpandoProps._scrubExpandos = new Thread(() =>
                            {
                                while (true)
                                {
                                    ExpandoProps._dictionaryLock.EnterWriteLock();
                                    //trim the dictionary of anything where the object has been collected.
                                    ExpandoProps._expandoProps = new Dictionary<WeakReference, IDictionary<string, object>>(
                                    ExpandoProps._expandoProps.Where(y => y.Key.IsAlive).ToDictionary(j => j.Key, k => k.Value));
                                    ExpandoProps._dictionaryLock.ExitWriteLock();

                                    //wait 15 seconds before attempting to clear again.
                                    Thread.Sleep(15000);
                                }
                            });
                            ExpandoProps._scrubExpandos.IsBackground = true;

                            ExpandoProps._scrubExpandos.Start();

                        }
                    }
                }
                #endregion
                ExpandoProps._dictionaryLock.EnterWriteLock();
                WeakReference reference = new WeakReference(obj);
                ExpandoProps._expandoProps[reference] = addedProps;
                ExpandoProps._dictionaryLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Set a property on the specified flyweight
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="property"></param>
        public static void SetProperty<T>(this IMongoFlyweight obj, String propertyName, T property)
        {
            ExpandoProps._dictionaryLock.EnterWriteLock();
            var dict = ExpandoProps._expandoProps.FirstOrDefault(y => y.Key.Target == (object)obj);

            if (dict.Key == null)
            {
                var reference = new WeakReference(obj);
                ExpandoProps._expandoProps[reference] = new Dictionary<String, object>(1);
                ExpandoProps._expandoProps[reference][propertyName] = property;
            }
            else
            {
                ExpandoProps._expandoProps[dict.Key][propertyName] = property;
            }
            ExpandoProps._dictionaryLock.ExitWriteLock();
        }

        /// <summary>
        /// Remove a property from the dictionary.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns>True if the property was found, false otherwise.</returns>
        public static bool DeleteProperty(this IMongoFlyweight obj, String propertyName)
        {
            bool retval = false;
            ExpandoProps._dictionaryLock.EnterWriteLock();
            var dict = ExpandoProps._expandoProps.FirstOrDefault(y => y.Key.Target == (object)obj);
            if (dict.Key != null && dict.Value != null && dict.Value.ContainsKey(propertyName))
            {
                retval = dict.Value.Remove(propertyName);
            }
            ExpandoProps._dictionaryLock.ExitWriteLock();
            return retval;
        }

        /// <summary>
        /// Provides a lookup for a particular property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T ExpandoProperty<T>(this IMongoFlyweight obj, String propertyName) where T : class
        {
            T retval = null;

            ExpandoProps._dictionaryLock.EnterReadLock();
            var dict = ExpandoProps._expandoProps.FirstOrDefault(y => y.Key.Target == (object)obj);
            if (dict.Key != null && dict.Value != null)
            {
                var value = dict.Value.FirstOrDefault(h => h.Key == propertyName);
                try
                {
                    retval = (T)value.Value;
                }
                catch
                {

                }
            }

            ExpandoProps._dictionaryLock.ExitReadLock();
            return retval;
        }



        /// <summary>
        /// Provides a lookup for a struct (remember to check this for "has value"
        /// </summary>
        /// <remarks>
        /// Just because you don't get anything back doesn't mean there's no prop of this type in the dictionary, 
        /// it's just of a different type than you have specified.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T? ExpandoValueProperty<T>(this IMongoFlyweight obj, String propertyName) where T : struct
        {
            T? retval = new T?();

            ExpandoProps._dictionaryLock.EnterReadLock();
            var dict = ExpandoProps._expandoProps.FirstOrDefault(y => y.Key.Target == (object)obj);
            if (dict.Key != null && dict.Value != null)
            {
                var value = dict.Value.FirstOrDefault(h => h.Key == propertyName);
                try
                {
                    retval = (T?)value.Value;
                }
                catch
                {

                }
            }

            ExpandoProps._dictionaryLock.ExitReadLock();
            return retval;
        }

    }
}
